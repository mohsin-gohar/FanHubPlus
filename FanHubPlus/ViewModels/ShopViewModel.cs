using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Store grid with category filter, search and sorting.</summary>
    public class ShopViewModel
    {
        public List<StoreProduct> Products { get; set; } = new();
        public List<StoreProduct> Featured { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public string? Category { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }                   // popular | price-asc | price-desc | rating
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TotalCount { get; set; }
    }
}
