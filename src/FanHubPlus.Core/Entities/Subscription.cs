using FanHubPlus.Core.Enums;

namespace FanHubPlus.Core.Entities;

public class Subscription
{
    public int SubscriptionId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int SubscriptionPlanId { get; set; }

    public SubscriptionStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? RenewsAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? PaymentTransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}