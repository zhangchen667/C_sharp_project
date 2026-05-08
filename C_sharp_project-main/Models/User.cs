using Microsoft.AspNetCore.Identity;

namespace MyPersonalSpace.Models;

public class User : IdentityUser
{
    public string? Nickname { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
