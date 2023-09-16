namespace Maxx.PluginVerticals.Application.Extensions;
public static class Extensions
{
    private static string? GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }
    public static bool IsDevelopmentEnvironment()
    {
        var environmentVariable = GetEnvironment();
        return !string.IsNullOrWhiteSpace(environmentVariable) && environmentVariable == "Development";
    }
}
