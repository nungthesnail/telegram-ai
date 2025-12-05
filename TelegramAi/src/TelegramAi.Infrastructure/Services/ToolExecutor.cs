using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class ToolExecutor : IToolExecutor
{
    private readonly AppDbContext _dbContext;

    public ToolExecutor(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> ExecuteToolAsync(string functionName, string arguments, Guid dialogId, CancellationToken cancellationToken)
    {
        switch (functionName)
        {
            case "publish_post":
                return await ExecutePublishPostAsync(arguments, dialogId, cancellationToken);
            default:
                return JsonSerializer.Serialize(new { error = $"Unknown function: {functionName}" });
        }
    }

    private async Task<string> ExecutePublishPostAsync(string arguments, Guid dialogId, CancellationToken cancellationToken)
    {
        try
        {
            var dialog = await _dbContext.Dialogs
                .Include(x => x.Channel)
                .FirstOrDefaultAsync(x => x.Id == dialogId, cancellationToken)
                ?? throw new InvalidOperationException("Dialog not found");

            var toolArgs = JsonSerializer.Deserialize<JsonElement>(arguments);
            if (!toolArgs.TryGetProperty("posts", out var postsArray))
            {
                return JsonSerializer.Serialize(new { error = "Posts array not found" });
            }

            var results = new List<object>();
            foreach (var postElement in postsArray.EnumerateArray())
            {
                var title = postElement.TryGetProperty("Title", out var titleProp) 
                    ? titleProp.GetString() 
                    : null;
                var content = postElement.TryGetProperty("Content", out var contentProp) 
                    ? contentProp.GetString() 
                    : throw new InvalidOperationException("Content is required");
                var scheduledAtUtc = postElement.TryGetProperty("ScheduledAtUtc", out var scheduledProp) 
                    && scheduledProp.ValueKind != JsonValueKind.Null
                    ? DateTimeOffset.Parse(scheduledProp.GetString()!) 
                    : (DateTimeOffset?)null;

                // Создаем пост в базе данных
                var post = new ChannelPost
                {
                    ChannelId = dialog.ChannelId,
                    Title = title,
                    Content = content,
                    Status = ChannelPostStatus.Suggested,
                    ScheduledAtUtc = scheduledAtUtc
                };

                _dbContext.ChannelPosts.Add(post);
                await _dbContext.SaveChangesAsync(cancellationToken);

                results.Add(new
                {
                    postId = post.Id,
                    title = post.Title,
                    status = "suggested",
                    scheduledAtUtc = post.ScheduledAtUtc
                });
            }

            return JsonSerializer.Serialize(new { success = true, posts = results });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}

