using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;

namespace MyPersonalSpace.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalPosts { get; set; }
    public int TotalPhotos { get; set; }
    public int TotalComments { get; set; }
    public int CommentsToReply { get; set; }

    public async Task OnGetAsync()
    {
        TotalPosts = await _context.Posts.CountAsync();
        TotalPhotos = await _context.Photos.CountAsync();
        TotalComments = await _context.Comments.CountAsync();
        CommentsToReply = await _context.Comments.CountAsync(c => string.IsNullOrEmpty(c.Reply));
    }
}
