namespace Maxx.PluginVerticals.Application.Extensions;
public static class Extensions
{
    public static bool IsDevelopmentEnvironment()
    {
        var environmentVariable = Shared.Extensions.Extensions.GetEnvironment();
        return !string.IsNullOrWhiteSpace(environmentVariable) && environmentVariable == "Development";
    }
}
