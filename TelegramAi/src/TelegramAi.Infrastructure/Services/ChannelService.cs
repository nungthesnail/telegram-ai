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

    public ChannelService(AppDbContext dbContext, ILogger<ChannelService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ChannelDto> CreateAsync(Guid userId, CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var channel = new Channel
        {
            OwnerId = userId,
            Title = request.Title,
            Description = request.Description,
            TelegramLink = request.TelegramLink,
            Category = request.Category
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

    public async Task<ChannelBotLinkDto> RequestBotLinkAsync(Guid userId, RequestBotLinkRequest request, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels
            .Include(x => x.BotLink)
            .FirstOrDefaultAsync(x => x.Id == request.ChannelId && x.OwnerId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Channel not found");

        var verificationCode = GenerateVerificationCode();

        if (channel.BotLink is null)
        {
            channel.BotLink = new ChannelBotLink
            {
                ChannelId = channel.Id,
                VerificationCode = verificationCode
            };
        }
        else
        {
            channel.BotLink.VerificationCode = verificationCode;
            channel.BotLink.VerifiedAtUtc = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Generated verification code for channel {ChannelId}", channel.Id);

        return channel.BotLink.ToDto();
    }

    public async Task<ChannelBotLinkDto> ConfirmBotLinkAsync(ConfirmBotLinkRequest request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.ChannelBotLinks
            .Include(x => x.Channel)
            .FirstOrDefaultAsync(x =>
                x.ChannelId == request.ChannelId &&
                x.VerificationCode == request.VerificationCode, cancellationToken)
            ?? throw new InvalidOperationException("Verification data invalid");

        link.TelegramBotId = request.TelegramBotId;
        link.TelegramChatId = request.TelegramChatId;
        link.VerifiedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Channel {ChannelId} verified via Telegram chat {ChatId}", request.ChannelId, request.TelegramChatId);

        return link.ToDto();
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


