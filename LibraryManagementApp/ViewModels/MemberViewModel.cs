using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;

namespace LibraryManagementApp.ViewModels;

public partial class MemberViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private readonly User _currentUser;
    private readonly Action _onLogout;

    // Catalog tab
    [ObservableProperty]
    private ObservableCollection<Book> _availableBooks = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private Book? _selectedBook;

    // My Loans tab
    [ObservableProperty]
    private ObservableCollection<Loan> _activeLoans = new();

    // Borrowing History
    [ObservableProperty]
    private ObservableCollection<Loan> _borrowingHistory = new();

    // Status
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string WelcomeMessage => $"Welcome, {_currentUser.FullName}!";

    public MemberViewModel(LibraryService libraryService, User currentUser, Action onLogout)
    {
        _libraryService = libraryService;
        _currentUser = currentUser;
        _onLogout = onLogout;

        Refresh();
    }

    public void Refresh()
    {
        RefreshCatalog();
        RefreshLoans();
        RefreshHistory();
    }

    [RelayCommand]
    private void Search()
    {
        var results = string.IsNullOrWhiteSpace(SearchQuery)
            ? _libraryService.GetAvailableBooks()
            : _libraryService.SearchBooks(SearchQuery).Where(b => b.IsAvailable).ToList();

        AvailableBooks = new ObservableCollection<Book>(results);
    }

    [RelayCommand]
    private void BorrowBook()
    {
        if (SelectedBook == null)
        {
            StatusMessage = "Please select a book to borrow.";
            return;
        }

        try
        {
            _libraryService.BorrowBook(_currentUser.Id, SelectedBook.Id);
            StatusMessage = $"Successfully borrowed \"{SelectedBook.Title}\"!";
            SelectedBook = null;
            Refresh();
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void ReturnBook(Loan loan)
    {
        try
        {
            _libraryService.ReturnBook(loan.Id);
            StatusMessage = $"Successfully returned \"{loan.BookTitle}\"!";
            Refresh();
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _onLogout();
    }

    private void RefreshCatalog()
    {
        var books = string.IsNullOrWhiteSpace(SearchQuery)
            ? _libraryService.GetAvailableBooks()
            : _libraryService.SearchBooks(SearchQuery).Where(b => b.IsAvailable).ToList();

        AvailableBooks = new ObservableCollection<Book>(books);
    }

    private void RefreshLoans()
    {
        ActiveLoans = new ObservableCollection<Loan>(
            _libraryService.GetActiveLoansForMember(_currentUser.Id));
    }

    private void RefreshHistory()
    {
        BorrowingHistory = new ObservableCollection<Loan>(
            _libraryService.GetBorrowingHistory(_currentUser.Id));
    }
}
