using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Albums;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public IList<Photo> Photos { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Photos = await _context.Photos
            .Include(p => p.Album)
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync();
    }
}
