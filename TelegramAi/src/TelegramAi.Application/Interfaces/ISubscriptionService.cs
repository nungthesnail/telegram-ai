using TelegramAi.Application.DTOs;
using TelegramAi.Application.Requests;

namespace TelegramAi.Application.Interfaces;

public interface ISubscriptionService
{
    Task<PaymentDto> StartSubscriptionAsync(Guid userId, StartSubscriptionRequest request, CancellationToken cancellationToken);
    Task<UserDto> RefreshSubscriptionStateAsync(Guid userId, CancellationToken cancellationToken);
}


