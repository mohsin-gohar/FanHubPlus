namespace FanHubPlus.Models
{
    /// <summary>Question / answer pair used by the FAQ page and the pricing FAQ band.</summary>
    public class FaqItem
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Topic { get; set; } = "General";     // General | Plans | Devices | Account | Billing
        public bool IsPopular { get; set; }
    }
}
