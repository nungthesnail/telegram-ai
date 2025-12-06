using System.Text.Json;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.DTOs.AiResponseEntities;
using TelegramAi.Domain.Entities;

namespace TelegramAi.Infrastructure.Extensions;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.TelegramUserId, user.Subscription?.ToDto());

    public static SubscriptionPlanDto ToDto(this SubscriptionPlan plan) =>
        new(plan.Id, plan.Name, plan.Description, plan.PriceRub, plan.TokensPerPeriod, plan.PeriodDays);

    public static UserSubscriptionDto ToDto(this UserSubscription subscription) =>
        new(subscription.Id, subscription.UserId, subscription.PlanId, subscription.LastRenewedAtUtc, 
            subscription.ExpiresAtUtc, subscription.Balance, subscription.LastReplenishAmount, subscription.Plan.ToDto());

    public static ChannelDto ToDto(this Channel channel) =>
        new(
            channel.Id,
            channel.Title,
            channel.Description,
            channel.TelegramLink,
            channel.Category,
            channel.AiDescription,
            channel.BotLink is { VerifiedAtUtc: not null },
            channel.BotLink?.ToDto(),
            channel.Posts.Select(ToDto).ToList());

    public static ChannelBotLinkDto ToDto(this ChannelBotLink link) =>
        new(link.Id, link.VerificationCode, link.TelegramChatId, link.TelegramBotId, link.VerifiedAtUtc);

    public static ChannelPostDto ToDto(this ChannelPost post) =>
        new(post.Id, post.TelegramPostId, post.Channel.TelegramChatId,
            post.Title, post.Content, post.Status, post.CreatedAtUtc, post.ScheduledAtUtc, post.PublishedAtUtc);

    public static DialogDto ToDto(this Dialog dialog) =>
        new(
            dialog.Id,
            dialog.ChannelId,
            dialog.UserId,
            dialog.Title,
            dialog.IsActive,
            dialog.Messages.Select(ToDto).OrderBy(m => m.CreatedAtUtc).ToList());

    public static DialogMessageDto ToDto(this DialogMessage message) =>
        new(
            message.Id,
            message.Sender,
            JsonSerializer.Deserialize<List<MessageEntity>>(message.ContentEntitiesJson)
                ?? [new ErrorMessageEntity("Не удалось прочитать сообщение")],
            message.CreatedAtUtc);

    public static PaymentDto ToDto(this Payment payment) =>
        new(payment.Id, payment.Amount, payment.Currency, payment.Status, payment.Provider, payment.CreatedAtUtc, payment.PaidAtUtc, payment.ExternalId);

    public static LlmModelInfoDto ToDto(this LlmModelInfo model)
        => new(model.Id, model.Name, model.ApiId, model.RequestTokenCost, model.ResponseTokenCost);
}


