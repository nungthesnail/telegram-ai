namespace TelegramAi.Api.Models;

public record EnsureUserRequest(string Email, string DisplayName);

public record SendDialogMessageRequest(long ModelId, string Message);


