namespace FanHubPlus.Services;

public interface IBookmarkService
{
    // Adds/removes a polymorphic bookmark. Returns true when the item is NOW bookmarked.
    // Throws KeyNotFoundException when the target item does not exist.
    Task<bool> ToggleAsync(string userId, Models.Enums.BookmarkType type, int itemId, string? note = null);
    Task<bool> IsBookmarkedAsync(string userId, Models.Enums.BookmarkType type, int itemId);

    // Removes one bookmark owned by the given user (false = not found / not theirs).
    Task<bool> RemoveAsync(string userId, int bookmarkId);
}
