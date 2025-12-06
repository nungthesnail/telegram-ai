using System.Text.Json;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Domain.Enums;

namespace TelegramAi.Infrastructure.Services;

public class ToolExecutor : IToolExecutor
{
    public async Task<string> ExecuteToolAsync(string functionName, string arguments, Guid dialogId, CancellationToken cancellationToken)
    {
        return functionName switch
        {
            "publish_post" => await ExecutePublishPostAsync(arguments, cancellationToken),
            _ => JsonSerializer.Serialize(new { error = $"Unknown function: {functionName}" })
        };
    }

    public event PublishPostsAsyncDelegate? OnPostsSuggested;

    private async Task<string> ExecutePublishPostAsync(string arguments, CancellationToken cancellationToken)
    {
        try
        {
            var toolArgs = JsonSerializer.Deserialize<JsonElement>(arguments);
            if (!toolArgs.TryGetProperty("posts", out var postsArray) || postsArray.ValueKind != JsonValueKind.Array)
            {
                return JsonSerializer.Serialize(new { error = "Posts array not found or posts is not an array" });
            }

            var toolResult = new List<object>();
            var posts = new List<ChannelPostDto>();
            foreach (var postElement in postsArray.EnumerateArray())
            {
                var title = postElement.TryGetProperty("Title", out var titleProp) 
                    ? titleProp.GetString() 
                    : null;
                
                var content = postElement.TryGetProperty("ContentEntitiesJson", out var contentProp) 
                    ? contentProp.GetString() ?? throw new InvalidOperationException("ContentEntitiesJson is required")
                    : throw new InvalidOperationException("ContentEntitiesJson is required");
                
                var scheduledAtUtc = postElement.TryGetProperty("ScheduledAtUtc", out var scheduledProp) 
                    && scheduledProp.ValueKind != JsonValueKind.Null
                    ? DateTimeOffset.Parse(scheduledProp.GetString()!) 
                    : (DateTimeOffset?)null;

                var post = new ChannelPostDto(Id: Guid.Empty, TelegramPostId: null, TelegramChatId: null,
                    Title: title, Content: content, Status: ChannelPostStatus.Suggested,
                    CreatedAtUtc: DateTimeOffset.Now, ScheduledAtUtc: scheduledAtUtc, PublishedAtUtc: null);
                
                posts.Add(post);
                toolResult.Add(new
                {
                    postId = post.Id,
                    title = post.Title,
                    status = "suggested",
                    scheduledAtUtc = post.ScheduledAtUtc
                });
            }

            var eventTask = OnPostsSuggested?.Invoke(posts, cancellationToken);
            if (eventTask is not null)
                await eventTask;
            
            return JsonSerializer.Serialize(new { success = true, posts = toolResult });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
