using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Albums;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DeleteModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public Photo Photo { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var photo = await _context.Photos.FirstOrDefaultAsync(m => m.Id == id);

        if (photo == null)
        {
            return NotFound();
        }

        Photo = photo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var photo = await _context.Photos.FindAsync(id);
        if (photo != null)
        {
            var filePath = Path.Combine(_environment.WebRootPath, photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                }
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
