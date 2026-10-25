using FanHubPlus.Application.Interfaces;
using FanHubPlus.Core.Entities;
using FanHubPlus.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<Subscription> _subscriptionRepository;

    public SubscriptionService(
        IRepository<SubscriptionPlan> planRepository,
        IRepository<Subscription> subscriptionRepository)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync(CancellationToken ct = default)
        => _planRepository.QueryNoTracking().ToListAsync(ct);

    public Task SubscribeAsync(string userId, int planId, CancellationToken ct = default)
    {
        var plan = _planRepository.QueryNoTracking().FirstOrDefault(p => p.SubscriptionPlanId == planId);
        if (plan is null)
        {
            return Task.CompletedTask;
        }

        var subscription = new Subscription
        {
            UserId = userId,
            SubscriptionPlanId = plan.SubscriptionPlanId,
            Status = Core.Enums.SubscriptionStatus.Active,
            StartedAt = DateTime.UtcNow,
            RenewsAt = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };

        _subscriptionRepository.Add(subscription);

        return Task.CompletedTask;
    }

    public Task CancelSubscriptionAsync(string userId, CancellationToken ct = default)
    {
        var subscription = _subscriptionRepository.QueryNoTracking()
            .FirstOrDefault(s => s.UserId == userId && s.Status == Core.Enums.SubscriptionStatus.Active);

        if (subscription is not null)
        {
            subscription.Status = Core.Enums.SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}