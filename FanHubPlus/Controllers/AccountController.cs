using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Models;
using FanHubPlus.ViewModels;

namespace FanHubPlus.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult SignIn(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);
            TempData["SuccessMessage"] = $"Welcome back! You are now signed in.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Account));
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View(new SignUpViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.Name,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} created a new account.", model.Email);
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "Your account has been created successfully!";
            return RedirectToAction(nameof(Account));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            TempData["SuccessMessage"] = "If an account exists for this email, we have sent a password reset link.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

        _logger.LogInformation("Password reset requested for {Email}", model.Email);
        TempData["SuccessMessage"] = $"Password reset link has been sent to {model.Email}.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token, string? email)
    {
        if (token == null || email == null)
        {
            return RedirectToAction(nameof(SignIn));
        }

        return View(new ResetPasswordViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            TempData["SuccessMessage"] = "If an account exists for this email, the password has been reset.";
            return RedirectToAction(nameof(SignIn));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset for {Email}", model.Email);
            TempData["SuccessMessage"] = "Your password has been reset successfully.";
            return RedirectToAction(nameof(SignIn));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Account(string tab = "overview")
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        return View(new ProfileViewModel
        {
            Name = user.FullName,
            Email = user.Email!
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Account(ProfileViewModel model, string tab = "overview")
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = model.Name;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Your profile has been updated.";
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return RedirectToAction(nameof(Account), new { tab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            return View("Account", new ProfileViewModel { Name = user?.FullName, Email = user?.Email! });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your password has been changed.";
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return RedirectToAction(nameof(Account), new { tab = "security" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        TempData["SuccessMessage"] = "You have been signed out.";
        return RedirectToAction(nameof(SignIn));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}