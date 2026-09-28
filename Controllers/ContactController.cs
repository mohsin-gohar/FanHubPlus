using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Models;
using FanHubPlus.Models.ViewModels;

namespace FanHubPlus.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Contact/SendMessage")]
        public IActionResult SendMessage(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process contact form
                ModelState.Clear();
                ModelState.AddModelError("", "Your message has been sent successfully!");
                return View("Index", model);
            }
            return View("Index", model);
        }
    }
}