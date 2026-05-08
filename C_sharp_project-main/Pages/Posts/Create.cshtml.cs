using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;
using System.Text.RegularExpressions;

namespace MyPersonalSpace.Pages.Posts;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CreateModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public Post Post { get; set; } = default!;

    public List<Category> Categories { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        Categories = await _context.Categories.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _context.Categories.ToListAsync();
            return Page();
        }

        if (Regex.IsMatch(Post.Content, @"<script[^>]*>|<\/script>", RegexOptions.IgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "内容不允许包含JavaScript脚本标签");
            Categories = await _context.Categories.ToListAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        Post.AuthorId = user?.Id;
        Post.CreatedAt = DateTime.Now;

        _context.Posts.Add(Post);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
