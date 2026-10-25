using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Core.Entities;

public class SubscriptionPlan
{
    public int SubscriptionPlanId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public decimal MonthlyPrice { get; set; }

    public decimal YearlyPrice { get; set; }

    public decimal LifetimePrice { get; set; }

    public int MaxStreamingQuality { get; set; }

    public int ConcurrentStreams { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}