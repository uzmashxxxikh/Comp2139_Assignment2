using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using WebApplication1.Services;
using System.Text.Encodings.Web;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation($"Attempting to register user with email: {model.Email}");
                    
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        ContactInformation = model.ContactInformation,
                        PreferredCategories = model.PreferredCategories,
                        EmailConfirmed = false // Email needs to be confirmed
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {model.Email} created successfully.");

                        // Assign RegularUser role to the new user
                        var roleResult = await _userManager.AddToRoleAsync(user, "RegularUser");
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation($"User {model.Email} assigned to RegularUser role.");
                        }
                        else
                        {
                            _logger.LogError($"Failed to assign RegularUser role to {model.Email}:");
                            foreach (var error in roleResult.Errors)
                            {
                                _logger.LogError($"- {error.Description}");
                            }
                        }

                        // Generate email confirmation token
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        
                        // Create a simple callback URL for testing purposes
                        string callbackUrl;
                        if (Url != null)
                        {
                            callbackUrl = Url.Action(
                                "ConfirmEmail",
                                "Account",
                                new { userId = user.Id, code = code },
                                protocol: Request.Scheme);
                        }
                        else
                        {
                            // Fallback for testing
                            callbackUrl = $"http://localhost/Account/ConfirmEmail?userId={user.Id}&code={code}";
                        }

                        // Send confirmation email
                        var emailSent = await _emailService.SendEmailConfirmationAsync(
                            model.Email,
                            HtmlEncoder.Default.Encode(callbackUrl));

                        if (emailSent)
                        {
                            _logger.LogInformation($"Confirmation email sent to {model.Email}.");
                            return RedirectToAction(nameof(RegisterConfirmation));
                        }
                        else
                        {
                            _logger.LogError($"Failed to send confirmation email to {model.Email}.");
                            ModelState.AddModelError(string.Empty, "Failed to send confirmation email. Please try again later.");
                            return View(model);
                        }
                    }
                    else
                    {
                        _logger.LogError($"Failed to create user {model.Email}:");
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"- {error.Description}");
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid model state for user registration: {model.Email}");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning($"- {error.ErrorMessage}");
                        }
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during registration for {model.Email}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"User with ID {userId} not found during email confirmation.");
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Email confirmed for user {user.Email}.");
                return View("ConfirmEmail");
            }
            else
            {
                _logger.LogError($"Failed to confirm email for user {user.Email}:");
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"- {error.Description}");
                }
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                _logger.LogInformation($"Login attempt for user: {model.Email}");
                
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {model.Email} logged in successfully.");
                        
                        // Check if user has Admin role
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        if (user != null)
                        {
                            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                            _logger.LogInformation($"User {model.Email} has Admin role: {isAdmin}");
                        }
                        
                        return LocalRedirect(returnUrl ?? Url.Content("~/"));
                    }
                    if (result.RequiresTwoFactor)
                    {
                        _logger.LogInformation($"User {model.Email} requires two-factor authentication.");
                        return RedirectToPage("./LoginWith2fa");
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning($"User account {model.Email} locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid login attempt for user {model.Email}.");
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }

                _logger.LogWarning($"Invalid model state for login: {model.Email}");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning($"- {error.ErrorMessage}");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during login for {model.Email}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again later.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Generate password reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                // Send password reset email
                await _emailService.SendPasswordResetAsync(
                    model.Email,
                    HtmlEncoder.Default.Encode(callbackUrl));

                _logger.LogInformation($"Password reset email sent to {model.Email}.");
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Password reset successful for user {model.Email}.");
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError($"Error resetting password for {model.Email}: {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation($"User {user.Email} has been promoted to Admin role.");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckRole(string roleName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { hasRole = false });
            }

            var hasRole = await _userManager.IsInRoleAsync(user, roleName);
            return Json(new { hasRole });
        }
    }
} 