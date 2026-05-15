namespace MyPersonalSpace.Models;

/// <summary>
/// 博客评论实体 - 原留言功能迁移到博客模块
/// </summary>
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsApproved { get; set; } = true;
    public string? Reply { get; set; }  // 博主回复

    // 关联博客
    public int PostId { get; set; }
    public virtual Post? Post { get; set; }

    // 发布用户（登录用户评论）
    public string? UserId { get; set; }
    public virtual User? User { get; set; }

    // 匿名用户信息（未登录用户评论）
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
}
