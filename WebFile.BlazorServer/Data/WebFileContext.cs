using Microsoft.EntityFrameworkCore;

namespace WebFile.BlazorServer.Data;

public class WebFileContext : DbContext
{
    public DbSet<UserModel> Users { get; set; }
    public DbSet<FileModel> FileModel { get; set; }

    public WebFileContext(DbContextOptions<WebFileContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>().HasMany(x => x.Files)
            .WithOne(x => x.Owner);
    }
}