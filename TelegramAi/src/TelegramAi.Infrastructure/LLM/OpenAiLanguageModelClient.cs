using System.ClientModel;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Options;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Options;

namespace TelegramAi.Infrastructure.LLM;

public class OpenAiLanguageModelClient : ILanguageModelClient
{
    private readonly OpenAiOptions _options;
    private readonly OpenAIClient _openAiClient;
    private readonly IToolExecutor _toolExecutor;

    public OpenAiLanguageModelClient(IOptions<OpenAiOptions> options, IToolExecutor toolExecutor)
    {
        _options = options.Value;
        var openApiOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(options.Value.ApiRoot)
        };
        _openAiClient = new OpenAIClient(new ApiKeyCredential(_options.ApiKey), openApiOptions);
        _toolExecutor = toolExecutor;
    }

    public async Task<AiResponseDto> GenerateResponseAsync(Guid dialogId,
        string modelId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        string? systemPrompt = null,
        string? toolsDescription = null,
        CancellationToken cancellationToken = default)
    {
        var chatClient = _openAiClient.GetChatClient(modelId);
        var messages = new List<ChatMessage>();

        // Добавляем системный промпт, если он есть
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(ChatMessage.CreateSystemMessage(systemPrompt));
        }

        // Добавляем историю сообщений
        foreach (var item in history)
        {
            // Пропускаем системные сообщения из истории, так как они уже добавлены
            if (item.Sender == DialogMessageSender.System)
                continue;
                
            messages.Add(item.Sender switch
            {
                DialogMessageSender.User => ChatMessage.CreateUserMessage(item.Content),
                DialogMessageSender.Assistant => ChatMessage.CreateAssistantMessage(item.Content),
                _ => ChatMessage.CreateSystemMessage(item.Content)
            });
        }

        messages.Add(ChatMessage.CreateUserMessage(userMessage));

        ChatCompletionOptions? options = null;
        
        // Добавляем функции, если есть описание инструментов
        if (!string.IsNullOrWhiteSpace(toolsDescription))
        {
            try
            {
                var toolsJson = JsonSerializer.Deserialize<JsonElement>(toolsDescription);
                if (toolsJson.ValueKind == JsonValueKind.Array)
                {
                    var tools = new List<ChatTool>();
                    foreach (var toolElement in toolsJson.EnumerateArray())
                    {
                        if (toolElement.TryGetProperty("type", out var typeProp) && 
                            typeProp.GetString() == "function" &&
                            toolElement.TryGetProperty("function", out var functionProp))
                        {
                            var functionName = functionProp.TryGetProperty("name", out var nameProp) 
                                ? nameProp.GetString() 
                                : null;
                            var description = functionProp.TryGetProperty("description", out var descProp) 
                                ? descProp.GetString() 
                                : null;
                            var parameters = functionProp.TryGetProperty("parameters", out var paramsProp) 
                                ? paramsProp 
                                : default;

                            if (!string.IsNullOrEmpty(functionName))
                            {
                                var functionTool = ChatTool.CreateFunctionTool(
                                    functionName,
                                    description ?? "",
                                    BinaryData.FromString(parameters.GetRawText()));
                                tools.Add(functionTool);
                            }
                        }
                    }
                    
                    if (tools.Count > 0)
                    {
                        options = new ChatCompletionOptions();
                        foreach (var tool in tools)
                        {
                            options.Tools.Add(tool);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Если не удалось распарсить JSON, продолжаем без функций
            }
        }

        // Циклическая обработка tool calls
        var totalInputTokens = 0;
        var totalOutputTokens = 0;
        var totalTokens = 0;
        var finalContent = "";
        var allToolCalls = new List<ToolCallDto>();
        const int maxIterations = 10; // Защита от бесконечного цикла
        var iteration = 0;

        while (iteration < maxIterations)
        {
            var response = options != null
                ? await chatClient.CompleteChatAsync(messages, options, cancellationToken)
                : await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            
            var completion = response.Value;
            
            // Суммируем использование токенов
            totalInputTokens += completion.Usage.InputTokenCount;
            totalOutputTokens += completion.Usage.OutputTokenCount;
            totalTokens += completion.Usage.TotalTokenCount;

            // Проверяем наличие tool calls
            if (completion.ToolCalls != null && completion.ToolCalls.Count > 0)
            {
                // Добавляем сообщение ассистента с tool calls в историю
                var assistantMessage = ChatMessage.CreateAssistantMessage(completion);
                messages.Add(assistantMessage);

                // Выполняем все tool calls
                var toolResults = new List<ChatMessage>();
                foreach (var toolCall in completion.ToolCalls)
                {
                    var toolCallType = toolCall.GetType();
                    var functionNameProp = toolCallType.GetProperty("FunctionName");
                    var functionArgumentsProp = toolCallType.GetProperty("FunctionArguments");
                    var idProp = toolCallType.GetProperty("Id");
                    
                    if (functionNameProp != null && idProp != null)
                    {
                        var functionName = functionNameProp.GetValue(toolCall)?.ToString();
                        var functionArguments = functionArgumentsProp?.GetValue(toolCall)?.ToString() ?? "{}";
                        var toolCallId = idProp.GetValue(toolCall)?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(functionName))
                        {
                            // Сохраняем информацию о tool call
                            allToolCalls.Add(new ToolCallDto(functionName, functionArguments));

                            // Выполняем инструмент
                            var toolResult = await _toolExecutor.ExecuteToolAsync(
                                functionName, 
                                functionArguments, 
                                dialogId, 
                                cancellationToken);

                            // Добавляем результат выполнения инструмента в историю
                            var toolResultMessage = ChatMessage.CreateToolMessage(toolCallId, toolResult);
                            toolResults.Add(toolResultMessage);
                        }
                    }
                }

                // Добавляем все результаты tool calls в историю
                messages.AddRange(toolResults);
                
                iteration++;
                continue; // Продолжаем цикл для получения следующего ответа
            }
            else
            {
                // Получен финальный ответ без tool calls
                finalContent = completion.Content.FirstOrDefault()?.Text ?? "Извините, я не смог сгенерировать ответ.";
                break;
            }
        }

        if (iteration >= maxIterations)
        {
            finalContent = "Достигнуто максимальное количество итераций обработки инструментов.";
        }
        
        return new AiResponseDto(
            finalContent,
            new TokenUsageDto(totalInputTokens, totalOutputTokens, totalTokens),
            allToolCalls.Count > 0 ? allToolCalls : null);
    }
}
