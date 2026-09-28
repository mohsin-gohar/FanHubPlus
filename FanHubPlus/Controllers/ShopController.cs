using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Models;
using FanHubPlus.Services;
using FanHubPlus.ViewModels;
using FanHubPlus.Services.Interfaces;
using FanHubPlus.DTOs;

namespace FanHubPlus.Controllers;

public class ShopController : Controller
{
    private readonly IStoreService _storeService;
    private readonly CartService _cart;

    public ShopController(IStoreService storeService, CartService cart)
    {
        _storeService = storeService;
        _cart = cart;
    }

    public async Task<IActionResult> Index(string? category, string? search, string? sortBy, decimal? minPrice, decimal? maxPrice)
    {
        var filter = new StoreFilter
        {
            Category = category,
            Search = search,
            SortBy = sortBy ?? "popular",
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };

        var result = await _storeService.GetPagedAsync(filter);

        var model = new ShopViewModel
        {
            Products = result.Items,
            Featured = result.AdditionalData?.Featured ?? new(),
            Categories = result.AdditionalData?.Categories ?? new(),
            Category = category,
            Search = search,
            SortBy = sortBy,
            MinPrice = minPrice ?? 0,
            MaxPrice = maxPrice ?? 500,
            TotalCount = result.TotalCount
        };

        return View(model);
    }

    public async Task<IActionResult> Product(int id)
    {
        var detail = await _storeService.GetDetailAsync(id);
        if (detail == null) return NotFound();

        var model = new ProductDetailsViewModel
        {
            Product = detail.Product,
            Related = detail.Related,
            SameCategory = detail.SameCategory,
            Gallery = detail.Gallery
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
        var all = (await _storeService.GetPagedAsync(new StoreFilter())).Items;

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