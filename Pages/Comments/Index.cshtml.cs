using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;
using System.Text.RegularExpressions;

namespace MyPersonalSpace.Pages.Comments;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Comment> Comments { get; set; } = default!;

    [BindProperty]
    public Comment NewComment { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Comments = await _context.Comments
            .Where(c => c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Comments = await _context.Comments
                .Where(c => c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Page();
        }

        if (!string.IsNullOrEmpty(NewComment.Email) &&
            !Regex.IsMatch(NewComment.Email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"))
        {
            ModelState.AddModelError("NewComment.Email", "请输入有效的邮箱地址");
            Comments = await _context.Comments
                .Where(c => c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Page();
        }

        var comment = new Comment
        {
            AuthorName = NewComment.AuthorName,
            Email = NewComment.Email,
            Content = NewComment.Content,
            CreatedAt = DateTime.Now,
            IsApproved = true
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
