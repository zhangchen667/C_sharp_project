using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Identity.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "请输入邮箱地址")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        [Display(Name = "邮箱")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("用户登录成功");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("用户账户被锁定");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "登录失败，请检查邮箱和密码是否正确");
                return Page();
            }
        }

        return Page();
    }
}
