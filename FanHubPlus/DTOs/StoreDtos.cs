namespace FanHubPlus.DTOs;

public class StoreFilter
{
    public string? Category { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "popular";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public string GetCacheKey()
    {
        return $"{Category}_{Search}_{SortBy}_{MinPrice}_{MaxPrice}";
    }
}

public class StoreDetailDto
{
    public StoreProduct Product { get; set; } = null!;
    public List<StoreProduct> Related { get; set; } = new();
    public List<StoreProduct> SameCategory { get; set; } = new();
    public List<StoreProduct> Gallery { get; set; } = new();
}

public class StoreListData
{
    public List<StoreProduct> Featured { get; set; } = new();
    public List<string> Categories { get; set; } = new();
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
    public StoreListData? AdditionalData { get; set; }
}