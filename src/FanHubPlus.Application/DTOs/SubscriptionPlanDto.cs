namespace FanHubPlus.Application.DTOs;

public class SubscriptionPlanDto
{
    public int SubscriptionPlanId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal MonthlyPrice { get; set; }

    public decimal YearlyPrice { get; set; }

    public decimal LifetimePrice { get; set; }

    public int MaxStreamingQuality { get; set; }

    public int ConcurrentStreams { get; set; }
}