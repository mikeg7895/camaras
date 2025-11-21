using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CameraClient.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CameraClient.Desktop;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Configurar servicios
        var services = new ServiceCollection();
        
        // Registrar TcpConnectionService como Singleton (una sola instancia compartida)
        services.AddSingleton<TcpConnectionService>();
        
        // Registrar otros servicios
        services.AddSingleton<AuthService>();
        services.AddSingleton<CameraService>();
        services.AddSingleton<VideoRecordingService>();
        services.AddSingleton<CameraDetectionService>();
        
        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}