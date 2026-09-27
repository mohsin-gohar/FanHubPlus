namespace FanHubPlus.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public int? StatusCode { get; set; }        // e.g. 404 or 500

    public string? Title { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
