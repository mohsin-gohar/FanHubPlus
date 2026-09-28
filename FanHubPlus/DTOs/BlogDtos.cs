namespace FanHubPlus.DTOs;

public class BlogFilter
{
    public string? Category { get; set; }
    public string Layout { get; set; } = "right";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 6;

    public string GetCacheKey()
    {
        return $"{Category}_{Layout}_{Page}_{PageSize}";
    }
}

public class BlogDetailDto
{
    public BlogPost Post { get; set; } = null!;
    public List<BlogPost> Related { get; set; } = new();
    public List<BlogPost> Latest { get; set; } = new();
    public List<BlogPost> Popular { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public string Sidebar { get; set; } = "right";
}

public class BlogListData
{
    public List<BlogPost> Latest { get; set; } = new();
    public List<BlogPost> Popular { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
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
    public BlogListData? AdditionalData { get; set; }
}