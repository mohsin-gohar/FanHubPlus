namespace FanHubPlus.ViewModels
{
    /// <summary>Drives the shared inner-page header band (eyebrow, title, lead, breadcrumb).</summary>
    public class PageHeaderViewModel
    {
        public string Eyebrow { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public List<(string Label, string? Url)> Breadcrumbs { get; set; } = new();
    }
}
