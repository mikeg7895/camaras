using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using System.Linq;

namespace Server.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private string _statusMessage = "Ready";
    private bool _isLoading;

    // Constructor para el diseñador
    public MainWindowViewModel()
    {
        _userService = null!;

        AllUsers = new ObservableCollection<User>();
        PendingUsers = new ObservableCollection<User>();

        // Comandos vacíos para diseñador
        LoadUsersCommand = ReactiveCommand.CreateFromTask(async () => await Task.CompletedTask);
        ApproveUserCommand = ReactiveCommand.CreateFromTask<User>(async (u) => await Task.CompletedTask);
        RejectUserCommand = ReactiveCommand.CreateFromTask<User>(async (u) => await Task.CompletedTask);
    }

    public MainWindowViewModel(IUserService userService)
    {
        _userService = userService;

        AllUsers = new ObservableCollection<User>();
        PendingUsers = new ObservableCollection<User>();

        // Comandos
        LoadUsersCommand = ReactiveCommand.CreateFromTask(ExecuteLoadUsersAsync);
        ApproveUserCommand = ReactiveCommand.CreateFromTask<User>(ExecuteApproveUserAsync);
        RejectUserCommand = ReactiveCommand.CreateFromTask<User>(ExecuteRejectUserAsync);

        // Cargar usuarios al iniciar
        LoadUsersCommand.Execute().Subscribe();
    }

    public ObservableCollection<User> AllUsers { get; }
    public ObservableCollection<User> PendingUsers { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ReactiveCommand<Unit, Unit> LoadUsersCommand { get; }
    public ReactiveCommand<User, Unit> ApproveUserCommand { get; }
    public ReactiveCommand<User, Unit> RejectUserCommand { get; }

    private async Task ExecuteLoadUsersAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading users...";

        try
        {
            var users = await _userService.GetAllAsync();
            var userList = users.ToList();

            AllUsers.Clear();
            PendingUsers.Clear();

            foreach (var user in userList)
            {
                AllUsers.Add(user);
                
                if (!user.Approved) // Approved = false significa pendiente de aprobación
                {
                    PendingUsers.Add(user);
                }
            }

            StatusMessage = $"Loaded {AllUsers.Count} users ({PendingUsers.Count} pending approval)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading users: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteApproveUserAsync(User user)
    {
        if (user == null) return;

        try
        {
            StatusMessage = $"Approving user {user.Username}...";

            user.Approved = true;
            _userService.Update(user);
            await _userService.SaveChangesAsync();

            // Actualizar listas
            PendingUsers.Remove(user);
            
            // Actualizar el usuario en AllUsers
            var userInList = AllUsers.FirstOrDefault(u => u.Id == user.Id);
            if (userInList != null)
            {
                userInList.Approved = true;
            }

            StatusMessage = $"User {user.Username} approved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error approving user: {ex.Message}";
        }
    }

    private async Task ExecuteRejectUserAsync(User user)
    {
        if (user == null) return;

        try
        {
            StatusMessage = $"Rejecting user {user.Username}...";

            user.Approved = false;
            _userService.Update(user);
            await _userService.SaveChangesAsync();

            // Si el usuario ya estaba aprobado, agregarlo de nuevo a pendientes
            if (!PendingUsers.Contains(user))
            {
                PendingUsers.Add(user);
            }

            StatusMessage = $"User {user.Username} rejected";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error rejecting user: {ex.Message}";
        }
    }
}
