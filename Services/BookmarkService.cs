using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Bookmark logic lives in a service because BOTH the detail pages (AJAX toggle)
/// and the My Bookmarks page need the exact same rules (unique user+type+item key).
/// The target row is verified before inserting, so a bookmark can never point at
/// a non-existent item (no orphan rows, no fake "saved" feedback).
/// </summary>
public class BookmarkService : IBookmarkService
{
    private readonly IRepository<Bookmark> _bookmarks;
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Article> _articles;
    private readonly IRepository<CharacterProfile> _characters;
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<MediaItem> _media;

    public BookmarkService(IRepository<Bookmark> bookmarks,
                           IRepository<Content> contents,
                           IRepository<Article> articles,
                           IRepository<CharacterProfile> characters,
                           IRepository<MerchandiseItem> merch,
                           IRepository<MediaItem> media)
    {
        _bookmarks = bookmarks;
        _contents = contents;
        _articles = articles;
        _characters = characters;
        _merch = merch;
        _media = media;
    }

    public async Task<bool> ToggleAsync(string userId, BookmarkType type, int itemId, string? note = null)
    {
        var existing = await _bookmarks.Query()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ItemType == type && b.ItemId == itemId);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(note)) // updating a note on an existing bookmark
            {
                existing.Note = note.Length > 500 ? note[..500] : note;
                await _bookmarks.SaveChangesAsync();
                return true;
            }

            _bookmarks.Remove(existing);
            await _bookmarks.SaveChangesAsync();
            return false;
        }

        // Only allow bookmarks that point at something real
        if (!await TargetExistsAsync(type, itemId))
            throw new KeyNotFoundException("That item no longer exists, so it cannot be bookmarked.");

        await _bookmarks.AddAsync(new Bookmark
        {
            UserId = userId,
            ItemType = type,
            ItemId = itemId,
            Note = note is { Length: > 500 } ? note[..500] : note,
            CreatedAt = DateTime.UtcNow
        });
        await _bookmarks.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsBookmarkedAsync(string userId, BookmarkType type, int itemId)
        => await _bookmarks.Query()
            .AsNoTracking()
            .AnyAsync(b => b.UserId == userId && b.ItemType == type && b.ItemId == itemId);

    // Removes one bookmark, but only when it belongs to the given user (IDOR guard)
    public async Task<bool> RemoveAsync(string userId, int bookmarkId)
    {
        var mark = await _bookmarks.Query()
            .FirstOrDefaultAsync(b => b.BookmarkId == bookmarkId && b.UserId == userId);

        if (mark is null) return false;

        _bookmarks.Remove(mark);
        await _bookmarks.SaveChangesAsync();
        return true;
    }

    private Task<bool> TargetExistsAsync(BookmarkType type, int itemId) => type switch
    {
        BookmarkType.Content => _contents.Query().AsNoTracking().AnyAsync(c => c.ContentId == itemId),
        BookmarkType.Article => _articles.Query().AsNoTracking().AnyAsync(a => a.ArticleId == itemId),
        BookmarkType.Character => _characters.Query().AsNoTracking().AnyAsync(c => c.CharacterId == itemId),
        BookmarkType.Merchandise => _merch.Query().AsNoTracking().AnyAsync(m => m.ItemId == itemId),
        BookmarkType.Video => _media.Query().AsNoTracking().AnyAsync(m => m.MediaId == itemId),
        _ => Task.FromResult(false)
    };
}
