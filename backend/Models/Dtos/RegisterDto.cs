using System.ComponentModel.DataAnnotations;

namespace MyPersonalSpace.Models.Dtos;

public class RegisterDto
{
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "昵称不能为空")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "昵称长度在2-20个字符之间")]
    public string Nickname { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度至少6个字符")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "两次密码不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
