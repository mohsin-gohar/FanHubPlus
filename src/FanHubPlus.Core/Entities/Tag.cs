using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// A free-form tag that can be attached to content (e.g. "Shonen", "Cyberpunk", "Live").
/// </summary>
public class Tag
{
    public int TagId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();
}