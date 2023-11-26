using Microsoft.EntityFrameworkCore;

namespace WebFile.BlazorServer.Data;

public class WebFileContext : DbContext
{
    public DbSet<UserModel> Users { get; set; }
    
    public WebFileContext(DbContextOptions<WebFileContext> options)
        : base(options) { }
}