using System.ClientModel;
using System.Collections.Concurrent;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Options;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.DTOs.AiResponseEntities;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Options;

namespace TelegramAi.Infrastructure.LLM;

public class OpenAiLanguageModelClient : ILanguageModelClient
{
    private readonly OpenAIClient _openAiClient;
    private readonly IToolExecutor _toolExecutor;
    
    private readonly ConcurrentStack<IEnumerable<ChannelPostDto>> _suggestedPosts = new();

    public OpenAiLanguageModelClient(IOptions<OpenAiOptions> options, IToolExecutor toolExecutor)
    {
        var openApiOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(options.Value.ApiRoot)
        };
        _openAiClient = new OpenAIClient(new ApiKeyCredential(options.Value.ApiKey), openApiOptions);
        _toolExecutor = toolExecutor;
        _toolExecutor.OnPostsSuggested += RememberSuggestedPosts;
    }

    private Task RememberSuggestedPosts(IEnumerable<ChannelPostDto> posts, CancellationToken _)
    {
        _suggestedPosts.Push(posts);
        return Task.CompletedTask;
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
        foreach (var msg in history)
        {
            messages.Add(msg.Sender switch
            {
                DialogMessageSender.User => ChatMessage.CreateUserMessage(msg.AsText),
                DialogMessageSender.Assistant => ChatMessage.CreateAssistantMessage(msg.AsText),
                DialogMessageSender.System => ChatMessage.CreateSystemMessage(msg.AsText),
                _ => throw new InvalidOperationException($"Unknown message sender: {msg.Sender}")
            });
        }

        messages.Add(ChatMessage.CreateUserMessage(userMessage));

        ChatCompletionOptions options = new();
        
        // Добавляем функции, если есть описание инструментов
        CreateChatCompletionOptions(toolsDescription, options);

        // Циклическая обработка tool calls
        var totalInputTokens = 0;
        var totalOutputTokens = 0;
        var messageEntities = new List<MessageEntity>();
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

            // Проверяем наличие tool calls
            if (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                await HandleToolCallsAsync(dialogId, cancellationToken, completion, messages);
                CheckSuggestedPosts(messageEntities);
            }
            else
            {
                // Получен финальный ответ без tool calls
                messageEntities.Add(new TextMessageEntity(completion.Content.FirstOrDefault()?.Text ?? ""));
                break;
            }
            iteration++;
        }

        if (iteration >= maxIterations)
        {
            messageEntities.Add(new TextMessageEntity(
                "Достигнуто максимальное количество итераций обработки инструментов."));
        }
        
        return new AiResponseDto(
            messageEntities,
            new TokenUsageDto(totalInputTokens, totalOutputTokens));
    }

    private async Task HandleToolCallsAsync(Guid dialogId, CancellationToken cancellationToken, ChatCompletion completion,
        List<ChatMessage> messages)
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
                    var toolResult = await _toolExecutor.ExecuteToolAsync(
                        functionName, 
                        functionArguments, 
                        dialogId, 
                        cancellationToken);

                    var toolResultMessage = ChatMessage.CreateToolMessage(toolCallId, toolResult);
                    toolResults.Add(toolResultMessage);
                }
            }
        }

        messages.AddRange(toolResults);
    }
    
    private void CheckSuggestedPosts(List<MessageEntity> messageEntities)
    {
        if (_suggestedPosts.IsEmpty)
            return;
        messageEntities.Add(new SuggestedPostsMessageEntity(_suggestedPosts.SelectMany(x => x).ToList()));
        _suggestedPosts.Clear();
    }

    private static void CreateChatCompletionOptions(
        string? toolsDescription, ChatCompletionOptions options)
    {
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
                        foreach (var tool in tools)
                        {
                            options.Tools.Add(tool);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore
            }
        }
    }
}
