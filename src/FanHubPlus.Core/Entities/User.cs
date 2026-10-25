using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// A registered fan user (Identity-backed user extended with a public profile).
/// </summary>
public class User
{
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastActiveAt { get; set; }

    public ICollection<UserCategory> UserCategories { get; set; } = new List<UserCategory>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}