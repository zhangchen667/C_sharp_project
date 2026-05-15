using System.ComponentModel.DataAnnotations;

namespace MyPersonalSpace.Models.Dtos;

public class ReplyDto
{
    [Required(ErrorMessage = "回复内容不能为空")]
    public string Reply { get; set; } = string.Empty;
}
