using Maxx.PluginVerticals.Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Maxx.PluginVerticals.Core.Database;
public class ApplicationDbContext: DbContext
{
    private static DbContextOptions GetDbContextOptionsForMigrations()
    {
        var configs = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        return
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(configs.GetConnectionString("Database"))
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
