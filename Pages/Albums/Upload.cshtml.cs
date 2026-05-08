using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;
using System.Text.RegularExpressions;

namespace MyPersonalSpace.Pages.Albums;

[Authorize(Roles = "Admin")]
public class UploadModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public UploadModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? PhotoFile { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public int AlbumId { get; set; }

    public List<Album> Albums { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Albums = await _context.Albums.ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PhotoFile == null || PhotoFile.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "请选择要上传的文件");
            Albums = await _context.Albums.ToListAsync();
            return Page();
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(PhotoFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError(string.Empty, "只支持上传图片文件（.jpg, .jpeg, .png, .gif, .webp）");
            Albums = await _context.Albums.ToListAsync();
            return Page();
        }

        if (PhotoFile.Length > 10 * 1024 * 1024)
        {
            ModelState.AddModelError(string.Empty, "文件大小不能超过10MB");
            Albums = await _context.Albums.ToListAsync();
            return Page();
        }

        try
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await PhotoFile.CopyToAsync(fileStream);
            }

            var photo = new Photo
            {
                FileName = PhotoFile.FileName,
                FilePath = "/uploads/" + uniqueFileName,
                FileSize = PhotoFile.Length,
                AlbumId = AlbumId,
                Description = Description,
                UploadedAt = DateTime.Now
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "文件上传失败，请重试");
            Albums = await _context.Albums.ToListAsync();
            return Page();
        }
    }
}
