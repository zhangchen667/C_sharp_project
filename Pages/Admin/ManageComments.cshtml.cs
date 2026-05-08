using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ManageCommentsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ManageCommentsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Comment> Comments { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Comments = await _context.Comments
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostReplyAsync(int id, string reply)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        comment.Reply = reply;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
