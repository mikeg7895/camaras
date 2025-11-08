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

public class RegisterViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    private readonly IScheduler _mainThreadScheduler;

    public RegisterViewModel()
    {
        _authService = new AuthService();
        _mainThreadScheduler = RxApp.MainThreadScheduler;
        
        RegisterCommand = ReactiveCommand.CreateFromTask(
            ExecuteRegisterAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        
        NavigateToLoginCommand = ReactiveCommand.Create(
            ExecuteNavigateToLogin,
            outputScheduler: RxApp.MainThreadScheduler);

        RegisterCommand.IsExecuting
            .ToProperty(this, x => x.IsLoading, out _isLoading, scheduler: RxApp.MainThreadScheduler);
    }

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => this.RaiseAndSetIfChanged(ref _successMessage, value);
    }

    public bool IsLoading => _isLoading.Value;

    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToLoginCommand { get; }

    public event EventHandler? RegisterSuccess;
    public event EventHandler? NavigateToLogin;

    private async Task ExecuteRegisterAsync()
    {
        SetErrorMessage(string.Empty);
        SetSuccessMessage(string.Empty);

        if (!ValidateForm())
            return;

        try
        {
            var request = new RegisterRequest
            {
                Username = Username,
                Email = Email,
                Password = Password,
                ConfirmPassword = ConfirmPassword
            };

            var response = await _authService.RegisterAsync(request);

            if (response.Success)
            {
                SetSuccessMessage("Registration successful! Redirecting to login...");
                await Task.Delay(1500);
                RaiseRegisterSuccess();
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

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            SetErrorMessage("Username is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            SetErrorMessage("Email is required");
            return false;
        }

        if (!IsValidEmail(Email))
        {
            SetErrorMessage("Invalid email format");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetErrorMessage("Password is required");
            return false;
        }

        if (Password.Length < 6)
        {
            SetErrorMessage("Password must be at least 6 characters");
            return false;
        }

        if (Password != ConfirmPassword)
        {
            SetErrorMessage("Passwords do not match");
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ExecuteNavigateToLogin()
    {
        RaiseNavigateToLogin();
    }

    private void SetErrorMessage(string message)
    {
        _mainThreadScheduler.Schedule(message, (scheduler, state) =>
        {
            ErrorMessage = state;
            return Disposable.Empty;
        });
    }

    private void SetSuccessMessage(string message)
    {
        _mainThreadScheduler.Schedule(message, (scheduler, state) =>
        {
            SuccessMessage = state;
            return Disposable.Empty;
        });
    }

    private void RaiseRegisterSuccess()
    {
        _mainThreadScheduler.Schedule(Unit.Default, (scheduler, _) =>
        {
            RegisterSuccess?.Invoke(this, EventArgs.Empty);
            return Disposable.Empty;
        });
    }

    private void RaiseNavigateToLogin()
    {
        _mainThreadScheduler.Schedule(Unit.Default, (scheduler, _) =>
        {
            NavigateToLogin?.Invoke(this, EventArgs.Empty);
            return Disposable.Empty;
        });
    }
}
