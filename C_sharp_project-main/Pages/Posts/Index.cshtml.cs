using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Pages.Posts;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Post> Posts { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    public List<Category> Categories { get; set; } = default!;

    public async Task OnGetAsync()
    {
        IQueryable<Post> postsQuery = _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsPublic);

        if (!string.IsNullOrEmpty(SearchString))
        {
            postsQuery = postsQuery.Where(p =>
                p.Title.Contains(SearchString) ||
                p.Content.Contains(SearchString));
        }

        if (CategoryId.HasValue)
        {
            postsQuery = postsQuery.Where(p => p.CategoryId == CategoryId.Value);
        }

        Posts = await postsQuery
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        Categories = await _context.Categories.ToListAsync();
    }
}
