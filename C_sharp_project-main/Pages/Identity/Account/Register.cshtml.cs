using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Identity.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "请输入邮箱地址")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        [Display(Name = "邮箱")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "请输入昵称")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "昵称长度必须在2到20个字符之间")]
        [Display(Name = "昵称")]
        public string Nickname { get; set; } = string.Empty;

        [Required(ErrorMessage = "请输入密码")]
        [StringLength(100, ErrorMessage = "{0}长度必须在{2}到{1}个字符之间", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                Nickname = Input.Nickname,
                CreatedAt = DateTime.Now,
                IsAdmin = false
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("用户创建成功");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, GetChineseErrorMessage(error));
            }
        }

        return Page();
    }

    private string GetChineseErrorMessage(IdentityError error)
    {
        return error.Code switch
        {
            "PasswordTooShort" => "密码太短，至少需要6个字符",
            "PasswordRequiresNonAlphanumeric" => "密码需要包含至少一个非字母数字的字符",
            "PasswordRequiresDigit" => "密码需要包含至少一个数字",
            "PasswordRequiresLower" => "密码需要包含至少一个小写字母",
            "PasswordRequiresUpper" => "密码需要包含至少一个大写字母",
            "DuplicateUserName" => "该邮箱已被注册",
            "InvalidUserName" => "用户名格式不正确",
            _ => error.Description
        };
    }
}
