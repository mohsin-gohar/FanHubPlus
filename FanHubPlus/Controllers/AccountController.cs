using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Data;
using FanHubPlus.ViewModels;

namespace FanHubPlus.Controllers;

/// <summary>Sign in, registration, password reset and the member area.</summary>
public class AccountController : Controller
{
    private readonly FanHubDbContext _db;

    public AccountController(FanHubDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult SignIn(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignIn(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = _db.Users.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "We could not find an account with that email address.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Welcome back, {user.Name}. You are signed in on this demo.";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Account));
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View(new SignUpViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (_db.Users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Email), "An account already exists with this email address.");
            return View(model);
        }

        _db.Users.Add(new Data.AppUser { Name = model.Name, Email = model.Email, Password = model.Password });
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Your account is ready. Sign in to pick up where you left off.";
        return RedirectToAction(nameof(SignIn));
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        TempData["SuccessMessage"] =
            $"If an account exists for {model.Email} we have sent a single use reset link. It expires in thirty minutes.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    [HttpGet]
    public IActionResult Account(string tab = "overview")
    {
        var profile = _db.Users.FirstOrDefault();
        return View(profile is null
            ? new ProfileViewModel()
            : new ProfileViewModel { Name = profile.Name, Email = profile.Email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Account(ProfileViewModel model, string tab = "overview")
    {
        if (!ModelState.IsValid) return View(model);

        TempData["SuccessMessage"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Account), new { tab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        TempData["SuccessMessage"] = "You have been signed out of the demo account.";
        return RedirectToAction(nameof(SignIn));
    }
}
