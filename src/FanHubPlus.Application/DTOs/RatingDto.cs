namespace FanHubPlus.Application.DTOs;

public class RatingDto
{
    public string UserId { get; set; } = string.Empty;

    public int Stars { get; set; }

    public string? Review { get; set; }
}