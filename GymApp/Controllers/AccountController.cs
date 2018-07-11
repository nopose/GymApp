using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
//using ASPNetCoreIdentity.Models.AccountViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace GymApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger _logger;

        public AccountController(
                    UserManager<AppUser> userManager,
                    SignInManager<AppUser> signInManager,
                    ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Manage(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new ManageViewModel());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Personal(ManageViewModel model, string returnUrl = null)
        {
            ModelState.Remove("OldPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");
            ModelState.Remove("Height");
            ModelState.Remove("Weight");
            ModelState.Remove("Age");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserID);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.UserName = model.Email;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
                return RedirectToLocal("Manage");
            }
            return View("Manage", model);
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(ManageViewModel model, string returnUrl = null)
        {
            ModelState.Remove("Height");
            ModelState.Remove("Weight");
            ModelState.Remove("Age");
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("Email");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserID);

                PasswordHasher<AppUser> ph = new PasswordHasher<AppUser>();
                var passwordResult = ph.VerifyHashedPassword(user, user.PasswordHash, model.OldPassword);

                if (passwordResult == PasswordVerificationResult.Success) {

                    string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(result);
                    return RedirectToLocal("Manage");
                }
            }
            return View("Manage", model); ;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Physical(ManageViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUser { FirstName = model.FirstName,
                                         LastName = model.LastName,
                                         UserName = model.Email,
                                         Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            //TODO Make lockout view
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
        #endregion
    }
}