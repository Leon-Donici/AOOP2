using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;

namespace LibraryManagementApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private readonly Action<User> _onLoginSuccess;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(LibraryService libraryService, Action<User> onLoginSuccess)
    {
        _libraryService = libraryService;
        _onLoginSuccess = onLoginSuccess;
    }

    [RelayCommand]
    private void Login()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter both username and password.";
            return;
        }

        var user = _libraryService.Login(Username, Password);
        if (user == null)
        {
            ErrorMessage = "Invalid username or password.";
            return;
        }

        _onLoginSuccess(user);
    }
}
