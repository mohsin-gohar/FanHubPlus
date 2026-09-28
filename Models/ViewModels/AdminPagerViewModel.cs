namespace FanHubPlus.Models.ViewModels;

/// <summary>
/// The admin pager row. It is a partial's own model (rather than reusing the
/// Bootstrap markup each list page had) so every list pages the same way, and
/// so lists that filter (feedback/submissions) can keep their own query string
/// by handing in their own URL builder.
/// </summary>
public class AdminPagerViewModel
{
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string PreviousUrl { get; set; } = "";
    public string NextUrl { get; set; } = "";

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static AdminPagerViewModel Build(int page, int totalPages, Func<int, string> urlFor)
    {
        var model = new AdminPagerViewModel { Page = page, TotalPages = totalPages };
        if (urlFor is not null)
        {
            model.PreviousUrl = urlFor(page - 1);
            model.NextUrl = urlFor(page + 1);
        }
        return model;
    }

    /// <summary>Pager for the generic admin list, keeping its search term.</summary>
    public static AdminPagerViewModel From<T>(AdminListViewModel<T> list)
        => Build(list.Page, list.TotalPages, list.PageUrl);
}
