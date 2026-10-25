namespace FanHubPlus.Core.Enums;

/// <summary>
/// Lifecycle state of a fan subscription / purchase.
/// </summary>
public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Cancelled,
    Expired
}