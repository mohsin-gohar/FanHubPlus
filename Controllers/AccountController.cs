using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FanHubPlus.Models.Entities;

namespace FanHubPlus.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/SignIn");
            return View(user);
        }
    }
}