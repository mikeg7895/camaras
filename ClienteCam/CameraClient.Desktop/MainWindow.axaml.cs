using Avalonia.Controls;
using CameraClient.Desktop.ViewModels;

namespace CameraClient.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}