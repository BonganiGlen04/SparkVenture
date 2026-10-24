using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkVenture.Data;

namespace SparkVenture.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? RegistrationSuccess { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");

        // Clear any external sign-in cookie before a fresh login attempt.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user is null)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Unable to load this account.");
                return Page();
            }

            _logger.LogInformation("User logged in.");

            // Respect a non-root local return URL (for example, a page the user
            // originally tried to access). Otherwise, send them to their role landing page.
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl) &&
                returnUrl != "/" &&
                returnUrl != Url.Content("~/"))
            {
                return LocalRedirect(returnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return LocalRedirect(Url.Action("Index", "Admin") ?? "/Admin");
            }

            if (await _userManager.IsInRoleAsync(user, "Reviewer"))
            {
                return LocalRedirect(
                    Url.Action("Index", "RegistrationReview") ?? "/RegistrationReview");
            }

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                return LocalRedirect(Url.Action("Index", "Dashboard") ?? "/Dashboard");
            }

            // Do not leave an authenticated session active if no supported
            // application role has been assigned.
            await _signInManager.SignOutAsync();
            ModelState.AddModelError(
                string.Empty,
                "Your account does not have an application role. Please contact support.");
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa",
                new { ReturnUrl = ReturnUrl, RememberMe = Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}
