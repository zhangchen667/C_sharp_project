namespace MyPersonalSpace.Models;

/// <summary>
/// 博客图片实体 - 存储博客的图文混排图片
/// </summary>
public class PostImage
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int PostId { get; set; }  // 关联博客ID
    public int DisplayOrder { get; set; } = 0;  // 显示顺序
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    public virtual Post? Post { get; set; }
}
