using FanHubPlus.Core.Entities;

namespace FanHubPlus.Application.Interfaces;

/// <summary>
/// Domain service contract for premium subscriptions and monetization.
/// </summary>
public interface ISubscriptionService
{
    Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync(CancellationToken ct = default);

    Task SubscribeAsync(string userId, int planId, CancellationToken ct = default);

    Task CancelSubscriptionAsync(string userId, CancellationToken ct = default);
}