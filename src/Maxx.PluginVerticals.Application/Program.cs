
using Carter;

using Maxx.PluginVerticals.Application;
using Maxx.PluginVerticals.Core.Extensions;

using Serilog;

Log.Logger = LoggingSetup.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
        
    builder.Configure(typeof(Program).Assembly);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.ApplyMigrations();
    }

    app.UseHttpsRedirection();

    app.MapCarter();
        
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
