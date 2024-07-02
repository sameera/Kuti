using Kuti.Windows.Common.VirtualDesktops;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kuti.Windows.Common;

public static class ServiceRegistrar
{
    public static IServiceCollection
        AddCommonServices(this IServiceCollection services) =>
            services
                .AddSingleton(GetAppMetadata())
                .AddSingleton(c => ConfigureLogger(c.GetService<AppMetadata>()))
                .AddSingleton<IDesktopsManager, DesktopsManager>()
        ;

    private static ILogger ConfigureLogger(AppMetadata? appMeta)
    {
        if (appMeta == null) throw new ArgumentNullException(nameof(appMeta), "Register AppMetadata in the container first.");

        string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appMeta.Company,
            appMeta.ProductName
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

    private static AppMetadata GetAppMetadata()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string? company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        string? productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        return new AppMetadata(productName, company);
    }
}
