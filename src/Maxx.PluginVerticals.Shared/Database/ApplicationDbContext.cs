using Maxx.PluginVerticals.Shared.Entities;

using Microsoft.EntityFrameworkCore;

namespace Maxx.PluginVerticals.Shared.Database;
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
        : this(GetDbContextOptionsForMigrations()) { }

    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public DbSet<Article> Articles { get; set; }
}
