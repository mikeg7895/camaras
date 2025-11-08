using System;
using CameraClient.Desktop.Models;

namespace CameraClient.Desktop.ViewModels;

public class LoginSuccessEventArgs : EventArgs
{
    public User User { get; }

    public LoginSuccessEventArgs(User user)
    {
        User = user;
    }
}
