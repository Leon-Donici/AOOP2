using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;

namespace LibraryManagementApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private readonly DataService _dataService;
    private readonly LibraryData _data;

    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public MainWindowViewModel(LibraryService libraryService, DataService dataService, LibraryData data)
    {
        _libraryService = libraryService;
        _dataService = dataService;
        _data = data;

        _currentView = CreateLoginViewModel();
    }

    // Parameterless constructor for design-time
    public MainWindowViewModel() : this(
        new LibraryService(new LibraryData()),
        new DataService(),
        new LibraryData())
    {
    }

    [RelayCommand]
    private void Save()
    {
        _dataService.Save(_data);
        StatusMessage = "Data saved successfully!";
    }

    [RelayCommand]
    private void Load()
    {
        var freshData = _dataService.Load();
        _data.Books = freshData.Books;
        _data.Users = freshData.Users;
        _data.Loans = freshData.Loans;
        StatusMessage = "Data loaded successfully!";

        // Refresh current view
        if (CurrentView is MemberViewModel member)
            member.Refresh();
        else if (CurrentView is LibrarianViewModel librarian)
            librarian.Refresh();
    }

    private LoginViewModel CreateLoginViewModel()
    {
        return new LoginViewModel(_libraryService, OnLoginSuccess);
    }

    private void OnLoginSuccess(User user)
    {
        if (user.Role == "Member")
        {
            CurrentView = new MemberViewModel(_libraryService, user, OnLogout);
        }
        else if (user.Role == "Librarian")
        {
            CurrentView = new LibrarianViewModel(_libraryService, user, OnLogout);
        }
        StatusMessage = $"Logged in as {user.FullName} ({user.Role})";
    }

    private void OnLogout()
    {
        CurrentView = CreateLoginViewModel();
        StatusMessage = "Logged out.";
    }
}
