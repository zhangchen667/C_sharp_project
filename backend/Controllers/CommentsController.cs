using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;
using MyPersonalSpace.Models.Dtos;
using System.Text.RegularExpressions;

namespace MyPersonalSpace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CommentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments()
    {
        var comments = await _context.Comments
            .Where(c => c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.AuthorName,
                c.Email,
                c.Content,
                c.Reply,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!string.IsNullOrEmpty(dto.Email) &&
            !Regex.IsMatch(dto.Email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"))
        {
            return BadRequest(new { success = false, message = "请输入有效的邮箱地址" });
        }

        var comment = new Comment
        {
            AuthorName = dto.AuthorName,
            Email = dto.Email,
            Content = dto.Content,
            CreatedAt = DateTime.Now,
            IsApproved = true
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "留言成功" });
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllComments()
    {
        var comments = await _context.Comments
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPut("{id}/reply")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReplyComment(int id, [FromBody] string reply)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound(new { success = false, message = "留言不存在" });

        comment.Reply = reply;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "回复成功" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound(new { success = false, message = "留言不存在" });

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "删除成功" });
    }
}
