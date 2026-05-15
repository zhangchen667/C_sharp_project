using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    #region 用户信息

    [HttpGet("{userId?}")]
    public async Task<IActionResult> GetProfile(string? userId = null)
    {
        // 如果未指定用户ID，获取当前登录用户
        if (string.IsNullOrEmpty(userId))
        {
            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "用户不存在" });

        var postCount = await _context.Posts.CountAsync(p => p.AuthorId == userId && p.IsPublic);
        var photoCount = await _context.Photos.CountAsync(p => p.UserId == userId);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Nickname,
            user.Avatar,
            user.Bio,
            user.CreatedAt,
            PostCount = postCount,
            PhotoCount = photoCount
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "用户不存在" });

        if (!string.IsNullOrEmpty(dto.Nickname))
            user.Nickname = dto.Nickname;

        if (!string.IsNullOrEmpty(dto.Bio))
            user.Bio = dto.Bio;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "更新失败", errors = result.Errors });

        return Ok(new { success = true, message = "更新成功" });
    }

    #endregion

    #region 用户博客

    [HttpGet("{userId}/posts")]
    public async Task<IActionResult> GetUserPosts(string userId, int page = 1, int pageSize = 10)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isOwner = currentUserId == userId;

        var query = _context.Posts
            .Include(p => p.Category)
            .Where(p => p.AuthorId == userId)
            .AsQueryable();

        // 非用户本人只能查看公开文章
        if (!isOwner)
            query = query.Where(p => p.IsPublic);

        var totalCount = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Content,
                p.CreatedAt,
                p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ImageCount = p.Images.Count,
                CommentCount = p.Comments.Count
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, posts });
    }

    #endregion

    #region 个人相册

    [HttpGet("{userId}/photos")]
    public async Task<IActionResult> GetUserPhotos(string userId)
    {
        var photos = await _context.Photos
            .Where(p => p.UserId == userId)
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

    [HttpPost("photos")]
    [Authorize]
    public async Task<IActionResult> UploadPhoto(IFormFile file, string? description = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "请选择要上传的图片" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { success = false, message = "只支持上传图片文件" });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { success = false, message = "文件大小不能超过10MB" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // 确保用户有默认相册
        var defaultAlbum = await _context.Albums
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Name == "默认相册");

        if (defaultAlbum == null)
        {
            defaultAlbum = new Album
            {
                Name = "默认相册",
                Description = "用户默认相册",
                UserId = userId
            };
            _context.Albums.Add(defaultAlbum);
            await _context.SaveChangesAsync();
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "albums");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        var photo = new Photo
        {
            FileName = file.FileName,
            FilePath = "/uploads/albums/" + uniqueFileName,
            FileSize = file.Length,
            AlbumId = defaultAlbum.Id,
            UserId = userId,
            Description = description,
            UploadedAt = DateTime.Now
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "上传成功", filePath = "/uploads/albums/" + uniqueFileName, photo.Id });
    }

    [HttpDelete("photos/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null)
            return NotFound(new { success = false, message = "照片不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (photo.UserId != userId)
            return Forbid("只能删除自己的照片");

        var filePath = Path.Combine(_environment.WebRootPath, photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "删除成功" });
    }

    #endregion
}

public class ProfileUpdateDto
{
    public string? Nickname { get; set; }
    public string? Bio { get; set; }
}
