namespace FanHubPlus.Models
{
    /// <summary>Merchandise / bundle sold through the FanHub store pages.</summary>
    public class StoreProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Merch";           // Merch | Audio | Devices | Bundles
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public int Sold { get; set; }
        public bool InStock { get; set; } = true;
        public string Badge { get; set; } = string.Empty;         // e.g. "-30%", "New"
        public string ArtTheme { get; set; } = "t1";
        public string ArtGlyph { get; set; } = "ri-shopping-bag-3-line";
        public string Sku { get; set; } = string.Empty;

        public string PriceDisplay => Price.ToString("0.00");
        public string OldPriceDisplay => OldPrice.HasValue ? OldPrice.Value.ToString("0.00") : "";
        public int DiscountPercent => OldPrice.HasValue && OldPrice.Value > Price
            ? (int)Math.Round((1 - (double)Price / (double)OldPrice.Value) * 100)
            : 0;
    }
}
