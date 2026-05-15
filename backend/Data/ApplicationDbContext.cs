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
    public DbSet<PostImage> PostImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<OperationLog> OperationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 分类种子数据
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "技术", Description = "技术相关文章" },
            new Category { Id = 2, Name = "生活", Description = "日常随笔" },
            new Category { Id = 3, Name = "学习", Description = "学习笔记" }
        );

        // PostImage 配置
        builder.Entity<PostImage>(entity =>
        {
            entity.HasOne(pi => pi.Post)
                  .WithMany(p => p.Images)
                  .HasForeignKey(pi => pi.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Comment 配置
        builder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Photo 配置 - 关联 User
        builder.Entity<Photo>(entity =>
        {
            entity.HasOne(p => p.User)
                  .WithMany(u => u.Photos)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Album 配置 - 关联 User
        builder.Entity<Album>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Albums)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OperationLog 配置
        builder.Entity<OperationLog>(entity =>
        {
            entity.ToTable("OperationLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TargetId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
