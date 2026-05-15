using Microsoft.AspNetCore.Identity;

namespace MyPersonalSpace.Models;

public class User : IdentityUser
{
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }  // 头像路径
    public string? Bio { get; set; }     // 个人简介
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 导航属性
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
