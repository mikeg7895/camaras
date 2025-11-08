using System;
using Avalonia.Threading;
using CameraClient.Desktop.Models;
using ReactiveUI;

namespace CameraClient.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentView = null!;

    public MainWindowViewModel()
    {
        Dispatcher.UIThread.Post(ShowLoginView);
    }

    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    private void ShowLoginView()
    {
        var loginViewModel = new LoginViewModel();
        loginViewModel.LoginSuccess += OnLoginSuccess;
        loginViewModel.NavigateToRegister += OnNavigateToRegister;

        SetCurrentView(loginViewModel);
    }

    private void ShowRegisterView()
    {
        var registerViewModel = new RegisterViewModel();
        registerViewModel.RegisterSuccess += OnRegisterSuccess;
        registerViewModel.NavigateToLogin += OnNavigateToLogin;

        SetCurrentView(registerViewModel);
    }

    private void ShowDashboardView(User user)
    {
        var dashboardViewModel = new DashboardViewModel(user);
        dashboardViewModel.LogoutRequested += OnLogoutRequested;

        SetCurrentView(dashboardViewModel);
    }

    private void OnLoginSuccess(object? sender, LoginSuccessEventArgs e)
    {
        if (sender is LoginViewModel login)
        {
            login.LoginSuccess -= OnLoginSuccess;
            login.NavigateToRegister -= OnNavigateToRegister;
        }

        Dispatcher.UIThread.Post(() => ShowDashboardView(e.User));
    }

    private void OnLogoutRequested(object? sender, System.EventArgs e)
    {
        if (sender is DashboardViewModel dashboard)
        {
            dashboard.LogoutRequested -= OnLogoutRequested;
        }

        Dispatcher.UIThread.Post(ShowLoginView);
    }

    private void OnNavigateToRegister(object? sender, System.EventArgs e)
    {
        if (sender is LoginViewModel login)
        {
            login.LoginSuccess -= OnLoginSuccess;
            login.NavigateToRegister -= OnNavigateToRegister;
        }

        Dispatcher.UIThread.Post(ShowRegisterView);
    }

    private void OnRegisterSuccess(object? sender, System.EventArgs e)
    {
        OnNavigateToLogin(sender, e);
    }

    private void OnNavigateToLogin(object? sender, System.EventArgs e)
    {
        if (sender is RegisterViewModel register)
        {
            register.RegisterSuccess -= OnRegisterSuccess;
            register.NavigateToLogin -= OnNavigateToLogin;
        }

        Dispatcher.UIThread.Post(ShowLoginView);
    }

    private void SetCurrentView(ViewModelBase viewModel)
    {
        if (_currentView is IDisposable disposable)
        {
            disposable.Dispose();
        }

        CurrentView = viewModel;
    }
}
