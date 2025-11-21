using System.Linq;
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
    private readonly ChatClient _chatClient;
    private readonly OpenAiOptions _options;

    public OpenAiLanguageModelClient(IOptions<OpenAiOptions> options)
    {
        _options = options.Value;
        var client = new OpenAIClient(_options.ApiKey);
        _chatClient = client.GetChatClient(_options.Model);
    }

    public async Task<AssistantResponseDto> GenerateResponseAsync(
        Guid dialogId,
        IReadOnlyCollection<DialogMessageDto> history,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>();

        foreach (var item in history)
        {
            messages.Add(item.Sender switch
            {
                DialogMessageSender.User => ChatMessage.CreateUserMessage(item.Content),
                DialogMessageSender.Assistant => ChatMessage.CreateAssistantMessage(item.Content),
                _ => ChatMessage.CreateSystemMessage(item.Content)
            });
        }

        messages.Add(ChatMessage.CreateUserMessage(userMessage));

        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var completion = response.Value;
        var content = completion.Content.FirstOrDefault()?.Text ?? "Извините, я не смог сгенерировать ответ.";

        return new AssistantResponseDto(content, Array.Empty<ChannelPostDto>(), null);
    }
}


