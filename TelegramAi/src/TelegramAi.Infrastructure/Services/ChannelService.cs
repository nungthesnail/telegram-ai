using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;
using TelegramAi.Domain.Entities;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class ChannelService : IChannelService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ChannelService> _logger;
    private readonly ITelegramChannelInfoProvider _telegramChannelInfoProvider;

    public ChannelService(AppDbContext dbContext, ILogger<ChannelService> logger, ITelegramChannelInfoProvider telegramChannelInfoProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _telegramChannelInfoProvider = telegramChannelInfoProvider;
    }

    public async Task<ChannelDto> CreateAsync(Guid userId, CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var channel = new Channel
        {
            OwnerId = userId
        };

        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await LoadDtoAsync(channel.Id, userId, cancellationToken) ?? channel.ToDto();
    }

    public async Task<ChannelDto> UpdateAsync(Guid userId, Guid channelId, UpdateChannelRequest request, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels.FirstOrDefaultAsync(x => x.Id == channelId && x.OwnerId == userId, cancellationToken)
                      ?? throw new InvalidOperationException("Channel not found");

        channel.Title = request.Title;
        channel.Description = request.Description;
        channel.TelegramLink = request.TelegramLink;
        channel.Category = request.Category;
        channel.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await LoadDtoAsync(channel.Id, userId, cancellationToken) ?? channel.ToDto();
    }

    public async Task<IReadOnlyCollection<ChannelDto>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var channels = await _dbContext.Channels
            .AsNoTracking()
            .Where(x => x.OwnerId == userId)
            .Include(x => x.BotLink)
            .Include(x => x.Posts)
            .ToListAsync(cancellationToken);

        return channels.Select(ch => ch.ToDto()).ToList();
    }

    public async Task<ChannelDto?> GetAsync(Guid userId, Guid channelId, CancellationToken cancellationToken)
    {
        return await LoadDtoAsync(channelId, userId, cancellationToken);
    }

    public async Task<ChannelDto> UpdateAiDescriptionAsync(Guid userId, Guid channelId, string aiDescription, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels.FirstOrDefaultAsync(x => x.Id == channelId && x.OwnerId == userId, cancellationToken)
                      ?? throw new InvalidOperationException("Channel not found");

        channel.AiDescription = aiDescription;
        channel.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await LoadDtoAsync(channel.Id, userId, cancellationToken) ?? channel.ToDto();
    }

    public async Task<ChannelDto> LinkChannelFromTelegramAsync(long telegramChatId, long telegramBotId, long? telegramUserId,
        CancellationToken cancellationToken)
    {
        // Получаем информацию о канале из Telegram
        var channelInfo = await _telegramChannelInfoProvider.GetChannelInfoAsync(telegramChatId, cancellationToken);

        // Ищем пользователя по TelegramUserId, если он указан
        User? user = null;
        if (telegramUserId.HasValue)
        {
            user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId.Value, cancellationToken);
        }

        // Проверяем, не существует ли уже канал с этим TelegramChatId
        var existingChannel = await _dbContext.Channels
            .Include(x => x.BotLink)
            .FirstOrDefaultAsync(x => x.BotLink != null && x.BotLink.TelegramChatId == telegramChatId, cancellationToken);

        Channel channel;
        if (existingChannel != null)
        {
            channel = existingChannel;
            // Обновляем информацию о канале из Telegram
            channel.Title = channelInfo.Title;
            channel.Description = channelInfo.Description;
            channel.TelegramLink = channelInfo.Link ?? $"https://t.me/{channelInfo.Username}";
        }
        else
        {
            // Создаем новый канал
            channel = new Channel
            {
                OwnerId = user?.Id ?? Guid.Empty, // Если пользователь найден, привязываем сразу
                Title = channelInfo.Title,
                Description = channelInfo.Description,
                TelegramLink = channelInfo.Username != null ? $"https://t.me/{channelInfo.Username}" : null
            };
            _dbContext.Channels.Add(channel);
        }

        // Создаем или обновляем связь с ботом
        if (channel.BotLink == null)
        {
            channel.BotLink = new ChannelBotLink
            {
                ChannelId = channel.Id,
                TelegramChatId = telegramChatId,
                TelegramBotId = telegramBotId,
                VerifiedAtUtc = user != null ? DateTime.UtcNow : null // Если пользователь найден, сразу подтверждаем
            };
            _dbContext.Add(channel.BotLink);
        }
        else
        {
            channel.BotLink.TelegramChatId = telegramChatId;
            channel.BotLink.TelegramBotId = telegramBotId;
            // Если пользователь найден и канал еще не подтвержден, подтверждаем
            if (user != null && channel.BotLink.VerifiedAtUtc == null)
            {
                channel.BotLink.VerifiedAtUtc = DateTime.UtcNow;
                if (channel.OwnerId == Guid.Empty)
                {
                    channel.OwnerId = user.Id;
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        if (user != null)
        {
            _logger.LogInformation("Channel {ChannelId} linked from Telegram chat {ChatId} for user {UserId}", channel.Id, telegramChatId, user.Id);
        }
        else
        {
            _logger.LogInformation("Channel {ChannelId} linked from Telegram chat {ChatId} (pending user confirmation)", channel.Id, telegramChatId);
        }

        return channel.ToDto();
    }

    public async Task<ChannelDto> ConfirmChannelLinkAsync(Guid userId, Guid channelId, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels
            .Include(x => x.BotLink)
            .FirstOrDefaultAsync(x => x.Id == channelId, cancellationToken)
            ?? throw new InvalidOperationException("Channel not found");

        if (channel.BotLink == null || channel.BotLink.TelegramChatId == null)
        {
            throw new InvalidOperationException("Channel bot link not found");
        }

        // Устанавливаем владельца, если еще не установлен
        if (channel.OwnerId == Guid.Empty)
        {
            channel.OwnerId = userId;
        }
        else if (channel.OwnerId != userId)
        {
            throw new InvalidOperationException("Channel already belongs to another user");
        }

        // Помечаем как подтвержденный
        channel.BotLink.VerifiedAtUtc = DateTime.UtcNow;
        channel.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Channel {ChannelId} confirmed by user {UserId}", channelId, userId);

        return await LoadDtoAsync(channel.Id, userId, cancellationToken) ?? channel.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid channelId, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels
            .FirstOrDefaultAsync(x => x.Id == channelId && x.OwnerId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Channel not found");

        _dbContext.Channels.Remove(channel);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Channel {ChannelId} deleted by user {UserId}", channelId, userId);
    }

    private async Task<ChannelDto?> LoadDtoAsync(Guid channelId, Guid ownerId, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels
            .AsNoTracking()
            .Include(x => x.BotLink)
            .Include(x => x.Posts)
            .FirstOrDefaultAsync(x => x.Id == channelId && x.OwnerId == ownerId, cancellationToken);

        return channel?.ToDto();
    }

    private static string GenerateVerificationCode()
    {
        Span<byte> bytes = stackalloc byte[5];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}


