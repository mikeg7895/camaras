using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using CameraClient.Desktop.Models;
using CameraClient.Desktop.Services;

namespace CameraClient.Desktop.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    private readonly IScheduler _mainThreadScheduler;

    public LoginViewModel()
    {
        _authService = new AuthService();
        _mainThreadScheduler = RxApp.MainThreadScheduler;
        
        LoginCommand = ReactiveCommand.CreateFromTask(
            ExecuteLoginAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        
        NavigateToRegisterCommand = ReactiveCommand.Create(
            ExecuteNavigateToRegister,
            outputScheduler: RxApp.MainThreadScheduler);

        LoginCommand.IsExecuting
            .ToProperty(this, x => x.IsLoading, out _isLoading, scheduler: RxApp.MainThreadScheduler);
    }

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool IsLoading => _isLoading.Value;

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToRegisterCommand { get; }

    public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;
    public event EventHandler? NavigateToRegister;

    private async Task ExecuteLoginAsync()
    {
        SetErrorMessage(string.Empty);
        
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetErrorMessage("Please enter email and password");
            return;
        }

        try
        {
            var request = new LoginRequest
            {
                Email = Email,
                Password = Password
            };

            var response = await _authService.LoginAsync(request);

            if (response.Success)
            {
                if (response.User is not null)
                {
                    RaiseLoginSuccess(response.User);
                }
                else
                {
                    SetErrorMessage("No user information returned by server");
                }
            }
            else
            {
                SetErrorMessage(response.Message);
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error: {ex.Message}");
        }
    }

    private void ExecuteNavigateToRegister()
    {
        RaiseNavigateToRegister();
    }

    private void SetErrorMessage(string message)
    {
        _mainThreadScheduler.Schedule(message, (scheduler, state) =>
        {
            ErrorMessage = state;
            return Disposable.Empty;
        });
    }

    private void RaiseLoginSuccess(User user)
    {
        _mainThreadScheduler.Schedule(user, (scheduler, state) =>
        {
            LoginSuccess?.Invoke(this, new LoginSuccessEventArgs(state));
            return Disposable.Empty;
        });
    }

    private void RaiseNavigateToRegister()
    {
        _mainThreadScheduler.Schedule(Unit.Default, (scheduler, _) =>
        {
            NavigateToRegister?.Invoke(this, EventArgs.Empty);
            return Disposable.Empty;
        });
    }
}
