using System.ComponentModel.DataAnnotations;

namespace MyPersonalSpace.Models.Dtos;

public class PostCreateDto
{
    [Required(ErrorMessage = "标题不能为空")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "内容不能为空")]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "请选择分类")]
    public int CategoryId { get; set; }

    public bool IsPublic { get; set; } = true;

    // 图片路径列表（上传后返回的路径）
    public List<string>? ImagePaths { get; set; }
}
