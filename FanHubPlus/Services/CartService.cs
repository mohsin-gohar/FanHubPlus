using System.Text.Json;
using FanHubPlus.ViewModels;

namespace FanHubPlus.Services
{
    /// <summary>
    /// Stores the cart in a plain cookie so the demo works without accounts or a
    /// database table. Line items hold only product id + quantity; prices are
    /// always re-read from the catalogue when the cart is rendered.
    /// </summary>
    public class CartService
    {
        private const string CookieName = "fanhub_cart";
        private readonly IHttpContextAccessor _accessor;

        private record StoredLine(int ProductId, int Quantity);

        public CartService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private List<StoredLine> Read()
        {
            var raw = _accessor.HttpContext?.Request.Cookies[CookieName];
            if (string.IsNullOrWhiteSpace(raw)) return new List<StoredLine>();
            try
            {
                return JsonSerializer.Deserialize<List<StoredLine>>(raw) ?? new List<StoredLine>();
            }
            catch (JsonException)
            {
                return new List<StoredLine>();
            }
        }

        private void Write(List<StoredLine> lines)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(14),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            _accessor.HttpContext?.Response.Cookies.Append(CookieName, JsonSerializer.Serialize(lines), options);
        }

        public void Add(int productId, int quantity = 1)
        {
            var lines = Read();
            var existing = lines.FirstOrDefault(l => l.ProductId == productId);
            if (existing is null) lines.Add(new StoredLine(productId, Math.Clamp(quantity, 1, 99)));
            else lines[lines.IndexOf(existing)] = new StoredLine(productId, Math.Min(99, existing.Quantity + quantity));
            Write(lines);
        }

        public void SetQuantity(int productId, int quantity)
        {
            var lines = Read();
            if (quantity <= 0) { Remove(productId); return; }
            var existing = lines.FirstOrDefault(l => l.ProductId == productId);
            if (existing is null) lines.Add(new StoredLine(productId, Math.Clamp(quantity, 1, 99)));
            else lines[lines.IndexOf(existing)] = new StoredLine(productId, Math.Clamp(quantity, 1, 99));
            Write(lines);
        }

        public void Remove(int productId)
        {
            Write(Read().Where(l => l.ProductId != productId).ToList());
        }

        public void Clear() => Write(new List<StoredLine>());

        public int Count => Read().Sum(l => l.Quantity);

        /// <summary>Raw contents of the cart as (productId, quantity) pairs.</summary>
        public IReadOnlyList<(int ProductId, int Quantity)> Entries() =>
            Read().Select(l => (l.ProductId, l.Quantity)).ToList();
    }
}
