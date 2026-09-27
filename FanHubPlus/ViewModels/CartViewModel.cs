using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>One line in the cart, joined with its product record.</summary>
    public class CartLine
    {
        public StoreProduct Product { get; set; } = new();
        public int Quantity { get; set; } = 1;
        public decimal LineTotal => Product.Price * Quantity;
    }

    /// <summary>Cart page + the summary block reused on checkout.</summary>
    public class CartViewModel
    {
        public List<CartLine> Lines { get; set; } = new();
        public string CouponCode { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public decimal Shipping { get; set; } = 49m;
        public decimal Tax { get; set; }

        public decimal Subtotal => Lines.Sum(l => l.LineTotal);
        public int ItemCount => Lines.Sum(l => l.Quantity);
        public bool IsEmpty => Lines.Count == 0;
        public decimal Total => Math.Max(0, Subtotal - Discount) + Shipping + Tax;
    }
}
