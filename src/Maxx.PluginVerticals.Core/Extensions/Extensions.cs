using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Maxx.PluginVerticals.Core.Extensions;
public static class Extensions
{
    public static IConfigurationRoot GetConfigurations()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>(HostDefaults.EnvironmentKey, environment)
            }!)
            .Build();
    }

    public static string GetConnectionString()
    {
        return GetConfigurations().GetConnectionString(Constants.Constants.ConnectionStringKey)!;
    }

}
