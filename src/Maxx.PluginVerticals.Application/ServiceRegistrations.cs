using System.Reflection;

using Carter;

using FluentValidation;

using Maxx.PluginVerticals.Core.Constants;
using Maxx.PluginVerticals.Core.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace Maxx.PluginVerticals.Application;

public static class ServiceRegistrations
{
    public static IHostApplicationBuilder Configure(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddDatabase(builder.Configuration)
            .AddMediatR(config => config.RegisterServicesFromAssembly(assembly))
            .AddCarter()
            .AddValidatorsFromAssembly(assembly);

        builder.Services.AddFeatureManagement().AddFeatureFilter<PercentageFilter>();

        return builder;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString(Constants.ConnectionStringKey);
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}
