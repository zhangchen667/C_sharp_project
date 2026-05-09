namespace MyPersonalSpace.Models;

public class Comment
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsApproved { get; set; } = true;
    public string? Reply { get; set; }
}
