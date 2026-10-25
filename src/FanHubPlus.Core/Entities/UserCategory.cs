using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// A fan's personal interest profile: which categories they follow/watch.
/// </summary>
public class UserCategory
{
    public string UserId { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}