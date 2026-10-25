using FanHubPlus.Core.Enums;

namespace FanHubPlus.Application.DTOs;

public class MediaItemDto
{
    public int MediaId { get; set; }

    public MediaType MediaType { get; set; }

    public string EmbedUrl { get; set; } = string.Empty;

    public string? Tag { get; set; }
}