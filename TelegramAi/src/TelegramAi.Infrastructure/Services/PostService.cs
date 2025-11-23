using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _dbContext;
    private readonly ITelegramPublisher _telegramPublisher;

    public PostService(AppDbContext dbContext, ITelegramPublisher telegramPublisher)
    {
        _dbContext = dbContext;
        _telegramPublisher = telegramPublisher;
    }

    public async Task CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken)
    {
        var channel = await EnsureChannelAsync(userId, request.ChannelId, cancellationToken);

        var post = new ChannelPost
        {
            ChannelId = channel.Id,
            Title = request.Title,
            Content = request.Content,
            ScheduledAtUtc = request.ScheduledAtUtc
        };

        _dbContext.ChannelPosts.Add(post);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Guid userId, Guid postId, UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await LoadPostAsync(userId, postId, cancellationToken);
        post.Title = request.Title;
        post.Content = request.Content;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.ScheduledAtUtc = request.ScheduledAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ScheduleAsync(Guid userId, Guid postId, SchedulePostRequest request, CancellationToken cancellationToken)
    {
        var post = await LoadPostAsync(userId, postId, cancellationToken);
        post.ScheduledAtUtc = request.ScheduledAtUtc;
        post.Status = ChannelPostStatus.Scheduled;
        post.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishAsync(Guid userId, Guid postId, CancellationToken cancellationToken)
    {
        var post = await LoadPostAsync(userId, postId, cancellationToken);
        var channel = await EnsureChannelAsync(userId, post.ChannelId, cancellationToken);

        var messageId = await _telegramPublisher.PublishAsync(channel.Id, post.Content, cancellationToken);

        post.Status = ChannelPostStatus.Published;
        post.PublishedAtUtc = DateTime.UtcNow;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.TelegramPostId = messageId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid postId, CancellationToken cancellationToken)
    {
        await _dbContext.ChannelPosts
            .Include(x => x.Channel)
            .Where(x => x.Id == postId && x.Channel.OwnerId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ChannelPostDto>> ListByChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken)
    {
        await EnsureChannelAsync(userId, channelId, cancellationToken);

        var posts = await _dbContext.ChannelPosts
            .AsNoTracking()
            .Where(x => x.ChannelId == channelId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Include(x => x.Channel)
            .ToListAsync(cancellationToken);

        return posts.Select(p => p.ToDto()).ToList();
    }

    private async Task<Channel> EnsureChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken)
    {
        return await _dbContext.Channels
                   .FirstOrDefaultAsync(x => x.Id == channelId && x.OwnerId == userId, cancellationToken)
               ?? throw new InvalidOperationException("Channel not found or access denied");
    }

    private async Task<ChannelPost> LoadPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken)
    {
        var post = await _dbContext.ChannelPosts
            .Include(x => x.Channel)
            .FirstOrDefaultAsync(x => x.Id == postId, cancellationToken)
            ?? throw new InvalidOperationException("Post not found");

        if (post.Channel.OwnerId != userId)
        {
            throw new InvalidOperationException("Access denied");
        }

        return post;
    }
}


