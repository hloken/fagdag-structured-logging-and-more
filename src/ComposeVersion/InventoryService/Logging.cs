using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

namespace InventoryService;

public static class Logging
{
    public static Logger AddLogging(this WebApplicationBuilder builder, string applicationName)
    {
        builder.Logging.ClearProviders(); // remove default logging providers
        var logger = CreateLogger(builder.Configuration, applicationName);
        builder.Logging.AddSerilog(logger);

        return logger;
    }

    private static Logger CreateLogger(IConfiguration configuration, string applicationName)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Information()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, shared:true)
            .WriteTo.File(new CompactJsonFormatter(), "Logs/log-.json.txt", rollingInterval: RollingInterval.Day, shared:true)
            .WriteTo.Seq("http://localhost:5341") // Ensure Seq is running or adjust the URL as needed
            .CreateLogger();
    }
}