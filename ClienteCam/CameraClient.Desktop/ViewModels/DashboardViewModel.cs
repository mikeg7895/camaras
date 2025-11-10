using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using CameraClient.Desktop.Models;
using CameraClient.Desktop.Services;
using static CameraClient.Desktop.Services.CameraDetectionService;

namespace CameraClient.Desktop.ViewModels;

public class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly CameraService _cameraService;
    private readonly IScheduler _mainThreadScheduler;
    private readonly User _currentUser;
    private readonly Guid _currentDeviceId;
    private string _statusMessage = string.Empty;
    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    private string _cameraName = string.Empty;
    private Camera? _cameraUnderEdit;
    private bool _isEditingCamera;
    private DetectedCamera? _selectedCamera;
    private readonly Dictionary<int, VideoRecordingService> _recordingServices = new();

    public DashboardViewModel(User user)
    {
        _currentUser = user;
        _cameraService = new CameraService();
        
        // Obtener el Device ID actual
        _currentDeviceId = DeviceIdentityService.GetOrCreateDeviceId();
        
        // Inicializar Device Info
        DeviceInfo = new DeviceInfoViewModel();
        _mainThreadScheduler = RxApp.MainThreadScheduler;
        
        Cameras = new ObservableCollection<Camera>();
        AvailableDeviceCameras = new ObservableCollection<DetectedCamera>();

        // Comandos
        LoadCamerasCommand = ReactiveCommand.CreateFromTask(
            ExecuteLoadCamerasAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        SaveCameraCommand = ReactiveCommand.CreateFromTask(
            ExecuteSaveCameraAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        EditCameraCommand = ReactiveCommand.Create<Camera>(
            ExecuteStartEditCamera,
            outputScheduler: RxApp.MainThreadScheduler);

        DeleteCameraCommand = ReactiveCommand.CreateFromTask<Camera>(
            ExecuteDeleteCameraAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        CancelEditCommand = ReactiveCommand.Create(
            ExecuteCancelEdit,
            outputScheduler: RxApp.MainThreadScheduler);

        LogoutCommand = ReactiveCommand.Create(
            ExecuteLogout,
            outputScheduler: RxApp.MainThreadScheduler);

        StartRecordingCommand = ReactiveCommand.CreateFromTask<Camera>(
            ExecuteStartRecordingAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        StopRecordingCommand = ReactiveCommand.CreateFromTask<Camera>(
            ExecuteStopRecordingAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        LoadCamerasCommand.IsExecuting
            .ToProperty(this, x => x.IsLoading, out _isLoading, scheduler: RxApp.MainThreadScheduler);

        // Cargar cámaras disponibles y registradas al iniciar
        InitializeAsync();
    }

    public User CurrentUser => _currentUser;
    public ObservableCollection<Camera> Cameras { get; }
    public ObservableCollection<DetectedCamera> AvailableDeviceCameras { get; }
    public DeviceInfoViewModel DeviceInfo { get; }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public string CameraName
    {
        get => _cameraName;
        set => this.RaiseAndSetIfChanged(ref _cameraName, value);
    }

    public DetectedCamera? SelectedCamera
    {
        get => _selectedCamera;
        set => this.RaiseAndSetIfChanged(ref _selectedCamera, value);
    }

    public Camera? CameraUnderEdit
    {
        get => _cameraUnderEdit;
        private set => this.RaiseAndSetIfChanged(ref _cameraUnderEdit, value);
    }

    public bool IsEditingCamera
    {
        get => _isEditingCamera;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isEditingCamera, value);
            this.RaisePropertyChanged(nameof(FormTitle));
            this.RaisePropertyChanged(nameof(PrimaryActionLabel));
        }
    }

    public bool IsLoading => _isLoading.Value;

    public ReactiveCommand<Unit, Unit> LoadCamerasCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCameraCommand { get; }
    public ReactiveCommand<Camera, Unit> EditCameraCommand { get; }
    public ReactiveCommand<Camera, Unit> DeleteCameraCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
    public ReactiveCommand<Camera, Unit> StartRecordingCommand { get; }
    public ReactiveCommand<Camera, Unit> StopRecordingCommand { get; }

    public event EventHandler? LogoutRequested;

    public string FormTitle => IsEditingCamera ? "Edit Camera" : "Add New Camera";
    public string PrimaryActionLabel => IsEditingCamera ? "Update Camera" : "Add Camera";

    private async void InitializeAsync()
    {
        try
        {
            // Cargar cámaras disponibles en el dispositivo
            var availableCameras = await CameraDetectionService.GetAvailableCamerasAsync();
            _mainThreadScheduler.Schedule(availableCameras, (scheduler, cameras) =>
            {
                AvailableDeviceCameras.Clear();
                foreach (var camera in cameras)
                {
                    AvailableDeviceCameras.Add(camera);
                }
                // Seleccionar la primera cámara por defecto
                if (AvailableDeviceCameras.Count > 0)
                {
                    SelectedCamera = AvailableDeviceCameras[0];
                }
                return Disposable.Empty;
            });

            // Cargar cámaras registradas
            await LoadCamerasCommand.Execute();
        }
        catch (Exception ex)
        {
            SetStatusMessage($"Error initializing: {ex.Message}");
        }
    }

    private async Task ExecuteLoadCamerasAsync()
    {
        SetStatusMessage("Loading cameras...");

        try
        {
            var response = await _cameraService.GetCamerasAsync(_currentUser.Id);

            if (response.Success && response.Cameras != null)
            {
                // Marcar las cámaras que pertenecen a este dispositivo
                foreach (var camera in response.Cameras)
                {
                    camera.BelongsToThisDevice = (camera.DeviceId == _currentDeviceId);
                }

                UpdateCamerasList(response.Cameras);
                SetStatusMessage($"Loaded {response.Cameras.Count} cameras");
            }
            else
            {
                SetStatusMessage(response.Message);
            }
        }
        catch (Exception ex)
        {
            SetStatusMessage($"Error: {ex.Message}");
        }
    }

    private async Task ExecuteSaveCameraAsync()
    {
        if (!ValidateCameraForm())
        {
            return;
        }

        try
        {
            CameraResponse response;

            if (IsEditingCamera && CameraUnderEdit is not null)
            {
                response = await _cameraService.UpdateCameraAsync(
                    CameraUnderEdit.Id,
                    CameraName);
            }
            else
            {
                if (SelectedCamera is null)
                {
                    SetStatusMessage("Please select a camera device");
                    return;
                }

                response = await _cameraService.RegisterCameraAsync(
                    CameraName,
                    _currentDeviceId,
                    SelectedCamera.Index,
                    _currentUser.Id);
            }

            if (response.Success)
            {
                await ExecuteLoadCamerasAsync();
                ExecuteCancelEdit();
                SetStatusMessage(response.Message);
            }
            else
            {
                SetStatusMessage(response.Message);
            }
        }
        catch (Exception ex)
        {
            SetStatusMessage($"Error: {ex.Message}");
        }
    }

    private void ExecuteStartEditCamera(Camera camera)
    {
        if (camera is null)
        {
            return;
        }

        _mainThreadScheduler.Schedule(camera, (scheduler, cam) =>
        {
            CameraUnderEdit = cam;
            CameraName = cam.Name;
            IsEditingCamera = true;
            return Disposable.Empty;
        });
    }

    private async Task ExecuteDeleteCameraAsync(Camera camera)
    {
        if (camera is null)
        {
            return;
        }

        try
        {
            var response = await _cameraService.DeleteCameraAsync(camera.Id);

            if (response.Success)
            {
                await ExecuteLoadCamerasAsync();
                if (CameraUnderEdit?.Id == camera.Id)
                {
                    ExecuteCancelEdit();
                }
                SetStatusMessage(response.Message);
            }
            else
            {
                SetStatusMessage(response.Message);
            }
        }
        catch (Exception ex)
        {
            SetStatusMessage($"Error: {ex.Message}");
        }
    }

    private void ExecuteCancelEdit()
    {
        _mainThreadScheduler.Schedule(Unit.Default, (scheduler, _) =>
        {
            CameraUnderEdit = null;
            CameraName = string.Empty;
            IsEditingCamera = false;
            return Disposable.Empty;
        });
    }

    private void ExecuteLogout()
    {
        RaiseLogoutRequested();
    }

    private async Task ExecuteStartRecordingAsync(Camera camera)
    {
        if (camera == null || !camera.BelongsToThisDevice)
        {
            SetStatusMessage("Cannot start recording: Camera not available on this device");
            return;
        }

        if (camera.IsRecording)
        {
            SetStatusMessage("Camera is already recording");
            return;
        }

        try
        {
            SetStatusMessage($"Starting recording for {camera.Name}...");

            // Crear servicio de grabación si no existe
            if (!_recordingServices.ContainsKey(camera.Id))
            {
                _recordingServices[camera.Id] = new VideoRecordingService();
            }

            var recordingService = _recordingServices[camera.Id];
            await recordingService.StartRecordingAsync(camera, _currentDeviceId);

            camera.IsRecording = true;
            SetStatusMessage($"Recording started for {camera.Name} (60-second segments)");
        }
        catch (Exception ex)
        {
            SetStatusMessage($"Error starting recording: {ex.Message}");
            Console.WriteLine($"Error starting recording: {ex}");
        }
    }

    private async Task ExecuteStopRecordingAsync(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        if (!camera.IsRecording)
        {
            SetStatusMessage("Camera is not recording");
            return;
        }

        try
        {
            SetStatusMessage($"Stopping recording for {camera.Name}...");

            if (_recordingServices.TryGetValue(camera.Id, out var recordingService))
            {
                await recordingService.StopRecordingAsync();
                recordingService.Dispose();
                _recordingServices.Remove(camera.Id);
            }

            camera.IsRecording = false;
            SetStatusMessage($"Recording stopped for {camera.Name}");
        }
        catch (Exception ex)
        {
            SetStatusMessage($"Error stopping recording: {ex.Message}");
            Console.WriteLine($"Error stopping recording: {ex}");
        }
    }

    private bool ValidateCameraForm()
    {
        if (string.IsNullOrWhiteSpace(CameraName))
        {
            SetStatusMessage("Camera name is required");
            return false;
        }

        if (!IsEditingCamera && SelectedCamera is null)
        {
            SetStatusMessage("Please select a camera device");
            return false;
        }

        return true;
    }

    private void SetStatusMessage(string message)
    {
        _mainThreadScheduler.Schedule(message, (scheduler, state) =>
        {
            StatusMessage = state;
            return Disposable.Empty;
        });
    }

    private void UpdateCamerasList(List<Camera> cameras)
    {
        _mainThreadScheduler.Schedule(cameras, (scheduler, state) =>
        {
            Cameras.Clear();
            foreach (var camera in state)
            {
                Cameras.Add(camera);
            }
            return Disposable.Empty;
        });
    }

    private void RaiseLogoutRequested()
    {
        _mainThreadScheduler.Schedule(Unit.Default, (scheduler, _) =>
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
            return Disposable.Empty;
        });
    }

    public void Dispose()
    {
        // Detener todas las grabaciones
        foreach (var service in _recordingServices.Values)
        {
            service.StopRecordingAsync().Wait();
            service.Dispose();
        }
        _recordingServices.Clear();

        _cameraService.Dispose();
        GC.SuppressFinalize(this);
    }
}
