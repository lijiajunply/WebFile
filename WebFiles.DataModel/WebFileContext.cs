using Microsoft.EntityFrameworkCore;

namespace WebFiles.DataModel;

public class WebFileContext : DbContext
{
    public DbSet<UserModel> Users { get; set; }
    public DbSet<FileModel> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .HasMany(e => e.FileModels)
            .WithOne()
            .IsRequired();
    }
}