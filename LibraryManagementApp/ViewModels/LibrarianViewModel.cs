using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;

namespace LibraryManagementApp.ViewModels;

public partial class LibrarianViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private readonly User _currentUser;
    private readonly Action _onLogout;

    // Catalog management
    [ObservableProperty]
    private ObservableCollection<Book> _allBooks = new();

    [ObservableProperty]
    private Book? _selectedBook;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Form fields for add/edit
    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private string _formAuthor = string.Empty;

    [ObservableProperty]
    private string _formIsbn = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    private Guid? _editingBookId;

    // Active loans
    [ObservableProperty]
    private ObservableCollection<Loan> _activeLoans = new();

    [ObservableProperty]
    private int _totalBorrowedCount;

    // Status
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string WelcomeMessage => $"Welcome, {_currentUser.FullName}!";

    public LibrarianViewModel(LibraryService libraryService, User currentUser, Action onLogout)
    {
        _libraryService = libraryService;
        _currentUser = currentUser;
        _onLogout = onLogout;

        Refresh();
    }

    public void Refresh()
    {
        RefreshCatalog();
        RefreshActiveLoans();
    }

    [RelayCommand]
    private void Search()
    {
        var results = string.IsNullOrWhiteSpace(SearchQuery)
            ? _libraryService.GetAllBooks()
            : _libraryService.SearchBooks(SearchQuery);

        AllBooks = new ObservableCollection<Book>(results);
    }

    [RelayCommand]
    private void AddBook()
    {
        if (string.IsNullOrWhiteSpace(FormTitle) || string.IsNullOrWhiteSpace(FormAuthor))
        {
            StatusMessage = "Title and Author are required.";
            return;
        }

        _libraryService.AddBook(FormTitle, FormAuthor, FormIsbn, FormDescription);
        StatusMessage = $"Book \"{FormTitle}\" added successfully!";
        ClearForm();
        RefreshCatalog();
    }

    [RelayCommand]
    private void EditBook()
    {
        if (SelectedBook == null)
        {
            StatusMessage = "Please select a book to edit.";
            return;
        }

        FormTitle = SelectedBook.Title;
        FormAuthor = SelectedBook.Author;
        FormIsbn = SelectedBook.Isbn;
        FormDescription = SelectedBook.Description;
        _editingBookId = SelectedBook.Id;
        IsEditing = true;
    }

    [RelayCommand]
    private void SaveEdit()
    {
        if (_editingBookId == null) return;

        if (string.IsNullOrWhiteSpace(FormTitle) || string.IsNullOrWhiteSpace(FormAuthor))
        {
            StatusMessage = "Title and Author are required.";
            return;
        }

        _libraryService.UpdateBook(_editingBookId.Value, FormTitle, FormAuthor, FormIsbn, FormDescription);
        StatusMessage = $"Book \"{FormTitle}\" updated successfully!";
        ClearForm();
        RefreshCatalog();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ClearForm();
    }

    [RelayCommand]
    private void DeleteBook()
    {
        if (SelectedBook == null)
        {
            StatusMessage = "Please select a book to delete.";
            return;
        }

        var title = SelectedBook.Title;
        var success = _libraryService.DeleteBook(SelectedBook.Id);
        if (success)
        {
            StatusMessage = $"Book \"{title}\" deleted successfully!";
            SelectedBook = null;
            RefreshCatalog();
        }
        else
        {
            StatusMessage = $"Cannot delete \"{title}\" — it has an active loan.";
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _onLogout();
    }

    private void ClearForm()
    {
        FormTitle = string.Empty;
        FormAuthor = string.Empty;
        FormIsbn = string.Empty;
        FormDescription = string.Empty;
        _editingBookId = null;
        IsEditing = false;
    }

    private void RefreshCatalog()
    {
        var books = string.IsNullOrWhiteSpace(SearchQuery)
            ? _libraryService.GetAllBooks()
            : _libraryService.SearchBooks(SearchQuery);

        AllBooks = new ObservableCollection<Book>(books);
    }

    private void RefreshActiveLoans()
    {
        var loans = _libraryService.GetAllActiveLoans();
        ActiveLoans = new ObservableCollection<Loan>(loans);
        TotalBorrowedCount = loans.Count;
    }
}
