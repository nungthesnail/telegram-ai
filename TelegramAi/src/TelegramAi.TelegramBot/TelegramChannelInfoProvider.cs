using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramAi.Application.Interfaces;
using TelegramAi.Infrastructure.Options;

namespace TelegramAi.TelegramBot;

public class TelegramChannelInfoProvider : ITelegramChannelInfoProvider
{
    private readonly IOptionsMonitor<TelegramOptions> _options;

    public TelegramChannelInfoProvider(IOptionsMonitor<TelegramOptions> options)
    {
        _options = options;
    }

    public async Task<TelegramChannelInfo> GetChannelInfoAsync(long chatId, CancellationToken cancellationToken)
    {
        var token = _options.CurrentValue.BotToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Telegram bot token is not configured");
        }

        var botClient = new TelegramBotClient(token);
        var chat = await botClient.GetChat(chatId, cancellationToken);

        var title = chat.Title ?? string.Empty;
        var description = chat.Description;
        var username = chat.Username;
        var link = chat.InviteLink;

        return new TelegramChannelInfo(title, description, username, link);
    }
}

