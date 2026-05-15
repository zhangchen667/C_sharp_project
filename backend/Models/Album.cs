namespace MyPersonalSpace.Models;

public class Album
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 归属用户
    public string UserId { get; set; } = string.Empty;
    public virtual User? User { get; set; }

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
