using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers
{
    public class TermsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}