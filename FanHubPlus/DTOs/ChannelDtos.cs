namespace FanHubPlus.DTOs;

public class ChannelFilter
{
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public string GetCacheKey()
    {
        return $"{Category}_{Page}_{PageSize}";
    }
}

public class ChannelListData
{
    public List<string> Genres { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public ChannelListData? AdditionalData { get; set; }
}