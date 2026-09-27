using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}