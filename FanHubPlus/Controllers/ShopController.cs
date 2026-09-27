using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanHubPlus.Data;
using FanHubPlus.Models;
using FanHubPlus.Services;
using FanHubPlus.ViewModels;

namespace FanHubPlus.Controllers;

/// <summary>Store grid, product page, cart and checkout.</summary>
public class ShopController : Controller
{
    private readonly FanHubDbContext _db;
    private readonly CartService _cart;

    public ShopController(FanHubDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<IActionResult> Index(string? category, string? search, string? sortBy, decimal? minPrice, decimal? maxPrice)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(p => p.Category == category);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }
        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        var all = await _db.Products.ToListAsync();
        var filtered = query.ToList();

        filtered = sortBy switch
        {
            "price-asc" => filtered.OrderBy(p => p.Price).ToList(),
            "price-desc" => filtered.OrderByDescending(p => p.Price).ToList(),
            "rating" => filtered.OrderByDescending(p => p.Rating).ToList(),
            _ => filtered.OrderByDescending(p => p.Sold).ToList()
        };

        var model = new ShopViewModel
        {
            Products = filtered,
            Featured = all.OrderByDescending(p => p.DiscountPercent).Take(3).ToList(),
            Categories = all.Select(p => p.Category).Distinct().OrderBy(c => c).ToList(),
            Category = category,
            Search = search,
            SortBy = sortBy,
            MinPrice = minPrice ?? 0,
            MaxPrice = maxPrice ?? 500,
            TotalCount = filtered.Count
        };

        return View(model);
    }

    public async Task<IActionResult> Product(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        var all = await _db.Products.ToListAsync();
        var model = new ProductDetailsViewModel
        {
            Product = product,
            Related = all.Where(p => p.Id != id).OrderByDescending(p => p.Sold).Take(4).ToList(),
            SameCategory = all.Where(p => p.Id != id && p.Category == product.Category).Take(3).ToList(),
            Gallery = new List<StoreProduct> { product }
                .Concat(all.Where(p => p.ArtTheme == product.ArtTheme && p.Id != id).Take(2))
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int id, int quantity = 1)
    {
        _cart.Add(id, quantity);
        TempData["SuccessMessage"] = "Added to your bag.";
        return RedirectToAction(nameof(Index));
    }

    private CartViewModel BuildCart(string? coupon)
    {
        var all = _db.Products.ToList();

        var model = new CartViewModel
        {
            Lines = _cart.Entries()
                .Where(e => all.Any(p => p.Id == e.ProductId))
                .Select(e => new CartLine
                {
                    Product = all.First(p => p.Id == e.ProductId),
                    Quantity = e.Quantity
                })
                .ToList(),
            CouponCode = coupon ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(coupon) && coupon.Trim().Equals("FANHUB10", StringComparison.OrdinalIgnoreCase))
        {
            model.Discount = Math.Round(model.Subtotal * 0.10m, 2);
        }
        model.Tax = model.IsEmpty ? 0 : Math.Round((model.Subtotal - model.Discount) * 0.05m, 2);
        model.Shipping = model.IsEmpty || model.Subtotal >= 500 ? 0 : 49m;

        return model;
    }

    [HttpGet]
    public IActionResult Cart(string? coupon) => View(BuildCart(coupon));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCart(int id, int quantity)
    {
        _cart.SetQuantity(id, quantity);
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int id)
    {
        _cart.Remove(id);
        return RedirectToAction(nameof(Cart));
    }

    [HttpGet]
    public IActionResult Checkout(string? coupon)
    {
        ViewBag.Cart = BuildCart(coupon);
        return View(new CheckoutViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutViewModel model, string? coupon)
    {
        var cart = BuildCart(coupon);

        if (cart.IsEmpty)
        {
            TempData["SuccessMessage"] = "Your bag is empty - add something before checking out.";
            return RedirectToAction(nameof(Cart));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Cart = cart;
            return View(model);
        }

        var orderId = "FH" + DateTime.Now.ToString("yyMMdd") + Random.Shared.Next(100, 999).ToString();
        TempData["SuccessMessage"] =
            $"Order {orderId} placed for ${cart.Total:0.00}. This is a demo store, so no payment was taken.";
        _cart.Clear();

        return RedirectToAction(nameof(OrderConfirmed), new { id = orderId, total = cart.Total });
    }

    [HttpGet]
    public IActionResult OrderConfirmed(string id, decimal total)
    {
        ViewBag.OrderId = id;
        ViewBag.Total = total;
        return View();
    }
}

