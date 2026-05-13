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
    private readonly OperationLogService _operationLogService;

    public PostsController(ApplicationDbContext context, UserManager<User> userManager, OperationLogService operationLogService)
    {
        _context = context;
        _userManager = userManager;
        _operationLogService = operationLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts(string? keyword = null, int? categoryId = null)
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

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
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
                AuthorName = p.Author != null ? p.Author.Nickname : null
            })
            .ToListAsync();

        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
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
            AuthorName = post.Author?.Nickname
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

        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            CategoryId = dto.CategoryId,
            AuthorId = userId,
            IsPublic = dto.IsPublic,
            CreatedAt = DateTime.Now
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // 记录操作日志
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                await _operationLogService.LogAsync(userId, "CreatePost", $"创建文章：{post.Title}", post.Id.ToString());
            }
            catch { /* 日志写入失败不影响主操作 */ }
        }

        return Ok(new { success = true, message = "发布成功", postId = post.Id });
    }

    /// <summary>
    /// 编辑文章 - 仅管理员可操作
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] PostUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 查找要编辑的文章
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return NotFound(new { success = false, message = "文章不存在" });

        // 安全校验：过滤XSS脚本
        if (Regex.IsMatch(dto.Content, @"<script[^>]*>|<\/script>", RegexOptions.IgnoreCase))
            return BadRequest(new { success = false, message = "内容不允许包含JavaScript脚本" });

        // 使用 Linq 更新文章属性
        post.Title = dto.Title;
        post.Content = dto.Content;
        post.CategoryId = dto.CategoryId;
        post.IsPublic = dto.IsPublic;
        post.UpdatedAt = DateTime.Now; // 设置更新时间

        _context.Posts.Update(post);
        await _context.SaveChangesAsync();

        // 记录操作日志
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                await _operationLogService.LogAsync(userId, "UpdatePost", $"编辑文章：{post.Title}", post.Id.ToString());
            }
            catch { }
        }

        return Ok(new { success = true, message = "更新成功", postId = post.Id });
    }

    /// <summary>
    /// 删除文章 - 仅管理员可操作
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        // 使用 Linq 查询文章
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return NotFound(new { success = false, message = "文章不存在" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (post.AuthorId != userId)
            return Forbid("只能删除自己发布的文章");

        var postTitle = post.Title; // 保存标题用于日志记录

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        // 记录操作日志
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                await _operationLogService.LogAsync(userId, "DeletePost", $"删除文章：{postTitle}", id.ToString());
            }
            catch { }
        }

        return Ok(new { success = true, message = "删除成功" });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync();

        return Ok(categories);
    }
}