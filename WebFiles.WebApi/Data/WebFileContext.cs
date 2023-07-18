using Microsoft.EntityFrameworkCore;

namespace WebFiles.WebApi.Data;

public class WebFileContext : DbContext
{
    public DbSet<UserDataModel> Users { get; set; }
    public DbSet<FileDataModel> Files { get; set; }

    public WebFileContext (DbContextOptions<WebFileContext> options)
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDataModel>()
            .HasMany(e => e.FileModels)
            .WithOne()
            .IsRequired();
    }
}