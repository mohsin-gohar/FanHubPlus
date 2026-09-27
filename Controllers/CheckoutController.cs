using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}