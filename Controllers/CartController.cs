using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}