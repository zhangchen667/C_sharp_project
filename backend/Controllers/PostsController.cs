using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;
using MyPersonalSpace.Models.Dtos;
using MyPersonalSpace.Services;
using System.Text.RegularExpressions;

namespace MyPersonalSpace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly OperationLogService _operationLogService;

    public PostsController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IWebHostEnvironment environment,
        OperationLogService operationLogService)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
        _operationLogService = operationLogService;
    }

    #region 博客文章

    [HttpGet]
    public async Task<IActionResult> GetPosts(string? keyword = null, int? categoryId = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsPublic)
            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

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
                p.UpdatedAt,
                p.CategoryId,
                p.AuthorId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                AuthorName = p.Author != null ? p.Author.Nickname : null,
                AuthorAvatar = p.Author != null ? p.Author.Avatar : null,
                ImageCount = p.Images.Count,
                CommentCount = p.Comments.Count
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, posts });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Include(p => p.Images.OrderBy(img => img.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound(new { success = false, message = "文章不存在" });

        return Ok(new
        {
            post.Id,
            post.Title,
            post.Content,
            post.CreatedAt,
            post.UpdatedAt,
            post.CategoryId,
            post.AuthorId,
            CategoryName = post.Category?.Name,
            AuthorName = post.Author?.Nickname,
            AuthorAvatar = post.Author?.Avatar,
            Images = post.Images.Select(img => new { img.Id, img.FilePath, img.FileName, img.DisplayOrder })
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] PostCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (Regex.IsMatch(dto.Content, @"<script[^>]*>|<\/script>", RegexOptions.IgnoreCase))
            return BadRequest(new { success = false, message = "内容不允许包含JavaScript脚本" });

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

        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            CategoryId = dto.CategoryId,
            AuthorId = userId,
            IsPublic = dto.IsPublic,
            CreatedAt = DateTime.Now
        };

        // 保存图片关联
        if (dto.ImagePaths != null && dto.ImagePaths.Any())
        {
            for (int i = 0; i < dto.ImagePaths.Count; i++)
            {
                var filePath = dto.ImagePaths[i];
                if (!string.IsNullOrEmpty(filePath))
                {
                    var fileName = Path.GetFileName(filePath);
                    var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    var fileSize = System.IO.File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;

                    post.Images.Add(new PostImage
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        FileSize = fileSize,
                        DisplayOrder = i
                    });
                }
            }
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        try
        {
            await _operationLogService.LogAsync(userId, "CreatePost", $"创建文章：{post.Title}", post.Id.ToString());
        }
        catch { /* 日志写入失败不影响主操作 */ }

        return Ok(new { success = true, message = "发布成功", postId = post.Id });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] PostUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return NotFound(new { success = false, message = "文章不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (post.AuthorId != userId)
            return Forbid("只能编辑自己发布的文章");

        if (Regex.IsMatch(dto.Content, @"<script[^>]*>|<\/script>", RegexOptions.IgnoreCase))
            return BadRequest(new { success = false, message = "内容不允许包含JavaScript脚本" });

        post.Title = dto.Title;
        post.Content = dto.Content;
        post.CategoryId = dto.CategoryId;
        post.IsPublic = dto.IsPublic;
        post.UpdatedAt = DateTime.Now;

        _context.Posts.Update(post);
        await _context.SaveChangesAsync();

        try
        {
            await _operationLogService.LogAsync(userId!, "UpdatePost", $"编辑文章：{post.Title}", post.Id.ToString());
        }
        catch { }

        return Ok(new { success = true, message = "更新成功", postId = post.Id });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound(new { success = false, message = "文章不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (post.AuthorId != userId)
            return Forbid("只能删除自己发布的文章");

        var postTitle = post.Title;

        // 删除图片文件
        foreach (var image in post.Images)
        {
            var filePath = Path.Combine(_environment.WebRootPath, image.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        try
        {
            await _operationLogService.LogAsync(userId!, "DeletePost", $"删除文章：{postTitle}", id.ToString());
        }
        catch { }

        return Ok(new { success = true, message = "删除成功" });
    }

    #endregion

    #region 图片上传

    [HttpPost("upload-image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
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
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "posts");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        var relativePath = "/uploads/posts/" + uniqueFileName;

        return Ok(new { success = true, message = "上传成功", filePath = relativePath, fileName = file.FileName });
    }

    #endregion

    #region 评论

    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetComments(int postId)
    {
        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId && c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                c.Reply,
                AuthorName = c.User != null ? c.User.Nickname : c.GuestName,
                AuthorAvatar = c.User != null ? c.User.Avatar : null
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost("comments")]
    public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var postExists = await _context.Posts.AnyAsync(p => p.Id == dto.PostId);
        if (!postExists)
            return NotFound(new { success = false, message = "文章不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = !string.IsNullOrEmpty(userId) ? await _userManager.FindByIdAsync(userId) : null;

        var comment = new Comment
        {
            PostId = dto.PostId,
            Content = dto.Content,
            CreatedAt = DateTime.Now,
            IsApproved = true,
            UserId = user?.Id,
            GuestName = user == null ? dto.GuestName : null,
            GuestEmail = user == null ? dto.GuestEmail : null
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "评论成功" });
    }

    [HttpDelete("comments/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound(new { success = false, message = "评论不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var post = await _context.Posts.FindAsync(comment.PostId);

        if (post?.AuthorId != userId && !User.IsInRole("Admin"))
            return Forbid("只能删除自己文章的评论");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "删除成功" });
    }

    [HttpPut("comments/{id}/reply")]
    [Authorize]
    public async Task<IActionResult> ReplyComment(int id, [FromBody] ReplyDto dto)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound(new { success = false, message = "评论不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var post = await _context.Posts.FindAsync(comment.PostId);

        if (post?.AuthorId != userId && !User.IsInRole("Admin"))
            return Forbid("只能回复自己文章的评论");

        comment.Reply = dto.Reply;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "回复成功" });
    }

    #endregion

    #region 分类

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync();

        return Ok(categories);
    }

    #endregion
}
