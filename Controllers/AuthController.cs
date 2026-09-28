using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;

namespace FanHubPlus.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Account/Login");
            var model = new ProfileViewModel
            {
                Name = user.Name,
                CurrentAvatarUrl = user.AvatarUrl,
                DarkMode = user.DarkMode,
                FontSize = user.FontSize
            };
            return View(model);
        }

        public async Task<IActionResult> Bookmarks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Account/Login");
            return View("Bookmarks");
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Account/Login");
            return View("History");
        }

        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Account/Login");
            return View("Settings");
        }
    }
}