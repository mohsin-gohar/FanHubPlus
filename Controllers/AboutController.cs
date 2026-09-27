using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}