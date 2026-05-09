using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;
using MyPersonalSpace.Models.Dtos;
using System.Text.RegularExpressions;

namespace MyPersonalSpace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public PostsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
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
                p.CategoryId,
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
            post.CategoryId,
            CategoryName = post.Category?.Name,
            AuthorName = post.Author?.Nickname
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
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

        return Ok(new { success = true, message = "发布成功", postId = post.Id });
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
