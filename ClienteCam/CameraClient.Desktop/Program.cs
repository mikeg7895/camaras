using Avalonia;
using Avalonia.ReactiveUI;
using System;
using CameraClient.Desktop.Services;

namespace CameraClient.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Inicializar o cargar el Device ID al iniciar la aplicación
        var deviceId = DeviceIdentityService.GetOrCreateDeviceId();
        Console.WriteLine($"Application Device ID: {deviceId}");
        Console.WriteLine("===========================================");
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
