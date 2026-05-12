namespace MyPersonalSpace.Models;

public class OperationLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? IpAddress { get; set; }

    public virtual User? User { get; set; }
}