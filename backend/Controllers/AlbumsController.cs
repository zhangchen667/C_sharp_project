using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AlbumsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AlbumsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("photos")]
    public async Task<IActionResult> GetPhotos()
    {
        var photos = await _context.Photos
            .OrderByDescending(p => p.UploadedAt)
            .Select(p => new
            {
                p.Id,
                p.FileName,
                p.FilePath,
                p.Description,
                p.UploadedAt
            })
            .ToListAsync();

        return Ok(photos);
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadPhoto(IFormFile file, string? description = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "请选择要上传的文件" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { success = false, message = "只支持上传图片文件" });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { success = false, message = "文件大小不能超过10MB" });

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var photo = new Photo
        {
            FileName = file.FileName,
            FilePath = "/uploads/" + uniqueFileName,
            FileSize = file.Length,
            AlbumId = 1,
            Description = description,
            UploadedAt = DateTime.Now
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "上传成功", filePath = "/uploads/" + uniqueFileName });
    }

    [HttpDelete("photos/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null)
            return NotFound(new { success = false, message = "照片不存在" });

        var filePath = Path.Combine(_environment.WebRootPath, photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "删除成功" });
    }
}
