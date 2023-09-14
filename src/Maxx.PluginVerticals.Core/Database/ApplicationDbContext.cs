using Maxx.PluginVerticals.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Maxx.PluginVerticals.Core.Database;
public class ApplicationDbContext: DbContext
{
    private static DbContextOptions GetDbContextOptionsForMigrations()
    {
        return
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(Extensions.Extensions.GetConnectionString())
                .Options;
    }

    public ApplicationDbContext()
        : base(GetDbContextOptionsForMigrations()) { }

    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public DbSet<Article> Articles { get; set; }
}
