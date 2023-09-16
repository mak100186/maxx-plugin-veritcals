using Maxx.PluginVerticals.Shared.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Maxx.PluginVerticals.Shared.Extensions;
public static class Extensions
{
    private const string ConnectionStringKey = "Database";

    private static string? GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }

    private static IConfigurationRoot GetConfigurations()
    {
        var environment = GetEnvironment() ?? "Development";

        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>(HostDefaults.EnvironmentKey, environment)
            }!)
            .Build();
    }

    public static string GetConnectionString()
    {
        return GetConfigurations().GetConnectionString(ConnectionStringKey)!;
    }
    

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString(ConnectionStringKey);
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    public static void ApplyMigrations(this IHost app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}
