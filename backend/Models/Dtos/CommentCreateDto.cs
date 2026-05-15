using System.ComponentModel.DataAnnotations;

namespace MyPersonalSpace.Models.Dtos;

public class CommentCreateDto
{
    [Required(ErrorMessage = "博客ID不能为空")]
    public int PostId { get; set; }

    // 匿名用户信息
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }

    [Required(ErrorMessage = "留言内容不能为空")]
    public string Content { get; set; } = string.Empty;
}
