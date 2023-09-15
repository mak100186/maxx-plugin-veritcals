using Serilog;
using Serilog.Extensions.Hosting;

namespace Maxx.PluginVerticals.Application.Extensions;

public static class LoggingSetup
{
    /// <summary>
    /// Builds a bootstrap logger which will ultimately replaced after app start.
    /// 
    /// <para>See: https://github.com/serilog/serilog-aspnetcore#two-stage-initialization</para>
    /// 
    /// <para>Pulls configuration from app settings and environment variables.
    /// Do not use fluent configuration as this will override anything in app settings.</para>
    /// </summary>
    public static ReloadableLogger CreateBootstrapLogger()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", true)
            .Build();

        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Builds a 'final' logger which is used throughout the applications lifetime.
    /// 
    /// <para>See: https://github.com/serilog/serilog-aspnetcore#two-stage-initialization</para>
    /// 
    /// <para>Pulls configuration from app settings and environment variables.
    /// Do not use fluent configuration as this will override anything in app settings.</para>
    /// </summary>
    public static IHostBuilder AddLogger(
        this IHostBuilder builder)
    {
        return builder
            .UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
            });
    }

    /// <summary>
    /// Adds serilog request logging middleware.
    ///
    /// <para>Important: This needs to occur before handlers such as MVC.</para>
    /// 
    /// <para>See: https://github.com/serilog/serilog-aspnetcore#request-logging</para>
    /// </summary>
    public static IApplicationBuilder AddRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseSerilogRequestLogging();
    }
}
