using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "技术", Description = "技术相关文章" },
            new Category { Id = 2, Name = "生活", Description = "日常随笔" },
            new Category { Id = 3, Name = "学习", Description = "学习笔记" }
        );

        builder.Entity<Album>().HasData(
            new Album { Id = 1, Name = "默认相册", Description = "默认上传相册" }
        );
    }
}
