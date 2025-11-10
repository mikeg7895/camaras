using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Interfaces;
using Server.Desktop.ViewModels;

namespace Server.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            
            // Obtener servicios desde DI
            if (Program.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider no está inicializado");
            }

            var userService = Program.ServiceProvider.GetRequiredService<IUserService>();
            DataContext = new MainWindowViewModel(userService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en MainWindow: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}