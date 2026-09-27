using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers
{
    public class FaqController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}