using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Product page: gallery, price block, tabs and cross-sell rail.</summary>
    public class ProductDetailsViewModel
    {
        public StoreProduct Product { get; set; } = new();
        public List<StoreProduct> Related { get; set; } = new();
        public List<StoreProduct> SameCategory { get; set; } = new();
        public List<StoreProduct> Gallery { get; set; } = new();
        public int Quantity { get; set; } = 1;
    }
}
