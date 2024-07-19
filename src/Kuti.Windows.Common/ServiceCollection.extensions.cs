using Kuti.Windows.Common.VirtualDesktops;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace Kuti.Windows.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection
        AddCommonServices(this IServiceCollection services) =>
            services
                .AddSingleton(c => ConfigureLogger())
                .AddSingleton<IDesktopsManager, DesktopsManager>()
        ;

    private static ILogger ConfigureLogger()
    {
        string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Config.Developer,
            Config.ProductName
        );

        string logFilePath = Path.Combine(
                    basePath,
                    "Logs",
                    "app.log"
                );

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
        return Log.Logger;
    }
}
