using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models;

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
            if (user == null) return Redirect("/Auth/SignIn");
            var model = new AccountViewModel
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl
            };
            return View(model);
        }

        public async Task<IActionResult> Bookmarks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/SignIn");
            return View("Bookmarks");
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/SignIn");
            return View("History");
        }

        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/SignIn");
            return View("Settings");
        }
    }
}