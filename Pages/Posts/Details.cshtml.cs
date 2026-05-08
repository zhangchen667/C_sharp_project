using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Posts;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Post Post { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        Post = post;
        return Page();
    }
}
