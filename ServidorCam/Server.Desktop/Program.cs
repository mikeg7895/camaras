using Avalonia;
using System;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Server.Application;
using Server.Infrastructure;
using System.IO;

namespace Server.Desktop;

class Program
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Configurar servicios
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al iniciar la aplicación: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Console.ReadLine();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        try
        {
            // Construir configuración
            var basePath = AppContext.BaseDirectory;
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Registrar configuración
            services.AddSingleton<IConfiguration>(configuration);

            Console.WriteLine("Configuración cargada correctamente");
            Console.WriteLine($"Connection String: {configuration.GetConnectionString("DefaultConnection")}");

            // Registrar servicios de Application e Infrastructure
            services.AddApplicationObjects();
            Console.WriteLine("Servicios de aplicación registrados");
            
            services.AddInfrastructureObjects(configuration);
            Console.WriteLine("Servicios de infraestructura registrados");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al configurar servicios: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
