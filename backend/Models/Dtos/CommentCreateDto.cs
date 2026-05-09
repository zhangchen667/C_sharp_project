using System.ComponentModel.DataAnnotations;

namespace MyPersonalSpace.Models.Dtos;

public class CommentCreateDto
{
    [Required(ErrorMessage = "昵称不能为空")]
    public string AuthorName { get; set; } = string.Empty;

    public string? Email { get; set; }

    [Required(ErrorMessage = "留言内容不能为空")]
    public string Content { get; set; } = string.Empty;
}
