using System;
using System.Reactive;
using ReactiveUI;
using CameraClient.Desktop.Services;

namespace CameraClient.Desktop.ViewModels;

public class DeviceInfoViewModel : ViewModelBase
{
    private string _deviceId = string.Empty;
    private string _deviceIdFilePath = string.Empty;
    private bool _isFirstRun;

    public DeviceInfoViewModel()
    {
        _isFirstRun = !DeviceIdentityService.DeviceIdFileExists();
        _deviceId = DeviceIdentityService.GetOrCreateDeviceId().ToString();
        _deviceIdFilePath = DeviceIdentityService.GetDeviceIdFilePath();

        CopyDeviceIdCommand = ReactiveCommand.Create(
            ExecuteCopyDeviceId,
            outputScheduler: RxApp.MainThreadScheduler);
    }

    public string DeviceId
    {
        get => _deviceId;
        set => this.RaiseAndSetIfChanged(ref _deviceId, value);
    }

    public string DeviceIdFilePath
    {
        get => _deviceIdFilePath;
        set => this.RaiseAndSetIfChanged(ref _deviceIdFilePath, value);
    }

    public bool IsFirstRun
    {
        get => _isFirstRun;
        set => this.RaiseAndSetIfChanged(ref _isFirstRun, value);
    }

    public string FirstRunMessage => IsFirstRun 
        ? "First application run - Device ID created" 
        : "Device ID loaded from file";

    public ReactiveCommand<Unit, Unit> CopyDeviceIdCommand { get; }

    private void ExecuteCopyDeviceId()
    {
        try
        {
            // Mostrar en consola
            Console.WriteLine($"Device ID: {DeviceId}");
            Console.WriteLine("Device ID displayed in console");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
