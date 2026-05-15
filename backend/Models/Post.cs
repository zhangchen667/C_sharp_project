namespace MyPersonalSpace.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? AuthorId { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // 导航属性
    public virtual Category? Category { get; set; }
    public virtual User? Author { get; set; }
    public virtual ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
