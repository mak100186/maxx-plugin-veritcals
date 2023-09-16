using System.Reflection;

using Carter;

using FluentValidation;

using LinqKit;

using Maxx.PluginVerticals.Shared.Extensions;

using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

using Serilog;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Maxx.PluginVerticals.Application.Dependency;

public static class ServiceRegistrations
{
    public static void Configure(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddDatabase(builder.Configuration)
            .AddFeatures(builder.Configuration, assembly);

        builder.Services.AddFeatureManagement().AddFeatureFilter<PercentageFilter>();
    }

    private static Action<SwaggerGenOptions> GetSwaggerGenAction(string[] paths)
    {
        return options =>
        {
            paths.ForEach(path =>
            {
                options.IncludeXmlComments(path, true);
            });
        };
    }

    private static void AddFeatures(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        var pluginNames = configuration.GetSection("features").Get<string[]>();

        var appFolder = new FileInfo(assembly.Location).DirectoryName!;
        if (pluginNames is not { Length: 0 })
        {
            var appsettingsPath = Path.Combine(appFolder, "appsettings.json");
            var configJson = File.Exists(appsettingsPath) ? $"FILE CONTENT FOLLOWS\n{File.ReadAllText(appsettingsPath)}" : $"CANNOT FIND {appsettingsPath}";
            var message = $"appsettings must define a features section listing all feature plugins to load. \n{configJson}";

            throw new(message);
        }

        var xmlDocFiles = new List<string>();
        var assemblies = new List<Assembly>();

        foreach (var pluginName in pluginNames)
        {
            var fullPathToPlugin = Extensions.Extensions.IsDevelopmentEnvironment()
                ? GetFullPathToPluginOnLocal(pluginName)
                : GetFullPathToPluginOnEnvironment(pluginName);

            if (!File.Exists(fullPathToPlugin))
            {
                EnumerationOptions enumerationOptions = new() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true };
                var fileList = Directory.GetFiles(appFolder, "*.dll", enumerationOptions);
                var message = $"Plugin path {fullPathToPlugin} does not resolve to a file.\nappFolder is {appFolder}\nVisible DLLs below this are\n  {string.Join("\n  ", fileList)}";

                throw new(message);
            }
            var assemblyLoader = new AssemblyLoader(fullPathToPlugin);

            assemblies.Add(assemblyLoader.Assembly);

            var xmlDocFile = fullPathToPlugin.Replace(".dll", ".xml");
            if (File.Exists(xmlDocFile))
            {
                xmlDocFiles.Add(xmlDocFile);
            }
        }

        var assembliesArray = assemblies.ToArray();

        services
            .AddCarter(new(assembliesArray))
            .AddMediatR(config => config.RegisterServicesFromAssemblies(assembliesArray))
            .AddValidatorsFromAssemblies(assembliesArray);

        services.Configure(GetSwaggerGenAction(xmlDocFiles.ToArray()));
    }

    private static string GetFullPathToPluginOnLocal(string relativePath)
    {
        var dllName = relativePath.Split('/').Last();

        // Navigate up to the solution root
        var root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(Program).Assembly.Location)!))!)!)!));

        var pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
        pluginLocation = Path.Combine(pluginLocation, $"bin\\Debug\\net8.0\\{dllName}.dll");
        Log.Information($"Loading plugin: {pluginLocation}");

        return pluginLocation;
    }

    private static string GetFullPathToPluginOnEnvironment(string relativePath)
    {
        var currentAssembly = Assembly.GetCallingAssembly();

        var assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        var pluginLocation = Path.GetFullPath(Path.Combine(assemblyPath, relativePath));

        Log.Information($"Loading plugin: {pluginLocation}");

        return pluginLocation;
    }
}
