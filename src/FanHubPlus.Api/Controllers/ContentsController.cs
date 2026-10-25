using FanHubPlus.Application.DTOs;
using FanHubPlus.Application.Interfaces;
using FanHubPlus.Core.Entities;
using FanHubPlus.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentsController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentsController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContentDto>> Get(int id, CancellationToken ct = default)
    {
        Content? content = await _contentService.GetContentByIdAsync(id, ct);
        if (content is null)
        {
            return NotFound();
        }

        return Ok(new ContentDto
        {
            ContentId = content.ContentId,
            CategoryId = content.CategoryId,
            Title = content.Title,
            Type = content.Type,
            Genre = content.Genre,
            Description = content.Description,
            ReleaseDate = content.ReleaseDate,
            PopularityScore = content.PopularityScore,
            ViewCount = content.ViewCount,
            ThumbnailUrl = content.ThumbnailUrl,
            Artist = content.Artist,
            AlbumName = content.AlbumName,
            Director = content.Director,
            RuntimeMinutes = content.RuntimeMinutes,
            AgeRating = content.AgeRating,
            Cast = content.Cast,
            CreatedAt = content.CreatedAt,
            CategoryName = content.Category?.Name ?? string.Empty,
            MediaItems = content.MediaItems.Select(m => new MediaItemDto
            {
                MediaId = m.MediaId,
                MediaType = m.MediaType,
                EmbedUrl = m.EmbedUrl,
                Tag = m.Tag
            }).ToList()
        });
    }
}