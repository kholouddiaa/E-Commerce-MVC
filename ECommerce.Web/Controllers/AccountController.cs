using ECommerce.BLL.Emails;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    IEmailService emailService,
    ILogger<AccountController> logger) : Controller
{
    private const string CustomerRoleName = "Customer";

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var roleResult = await EnsureCustomerRoleExistsAsync();
        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName.Trim(),
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim()
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, CustomerRoleName);
        if (!addToRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddIdentityErrors(addToRoleResult);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                var emailBody = EmailTemplate.Create(
                    "Welcome",
                    "Welcome to E-Commerce MVC",
                    $"<p>Hello {EmailTemplate.Encode(user.FullName)},</p><p>Your account has been created successfully.</p>");

                await emailService.SendHtmlEmailAsync(user.Email, "Welcome to E-Commerce MVC", emailBody);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to send welcome email for user {UserId}.", user.Id);
            }
        }

        TempData["SuccessMessage"] = "Registration completed successfully. Please log in with your new account.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByNameAsync(model.UserName.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your username and password.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Your account is locked. Please try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your username and password.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Index", "Home");
    }

    private async Task<IdentityResult> EnsureCustomerRoleExistsAsync()
    {
        if (await roleManager.RoleExistsAsync(CustomerRoleName))
        {
            return IdentityResult.Success;
        }

        return await roleManager.CreateAsync(new IdentityRole(CustomerRoleName));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
