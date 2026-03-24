using System;
using System.Collections.Generic;
using System.Linq;
using LibraryManagementApp.Models;

namespace LibraryManagementApp.Services;

public class LibraryService
{
    private readonly LibraryData _data;

    public LibraryService(LibraryData data)
    {
        _data = data;
    }

    // --- Authentication ---

    public User? Login(string username, string password)
    {
        var user = _data.Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return null;

        return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }

    // --- Book operations ---

    public List<Book> GetAllBooks()
    {
        return _data.Books.ToList();
    }

    public List<Book> GetAvailableBooks()
    {
        return _data.Books.Where(b => b.IsAvailable).ToList();
    }

    public List<Book> SearchBooks(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _data.Books.ToList();

        return _data.Books.Where(b =>
            b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            b.Isbn.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Book? GetBookById(Guid id)
    {
        return _data.Books.FirstOrDefault(b => b.Id == id);
    }

    public void AddBook(string title, string author, string isbn, string description)
    {
        var book = new Book
        {
            Title = title,
            Author = author,
            Isbn = isbn,
            Description = description,
            IsAvailable = true
        };
        _data.Books.Add(book);
    }

    public void UpdateBook(Guid id, string title, string author, string isbn, string description)
    {
        var book = _data.Books.FirstOrDefault(b => b.Id == id);
        if (book == null) return;

        book.Title = title;
        book.Author = author;
        book.Isbn = isbn;
        book.Description = description;
    }

    public bool DeleteBook(Guid id)
    {
        var hasActiveLoan = _data.Loans.Any(l => l.BookId == id && l.IsActive);
        if (hasActiveLoan)
            return false;

        var book = _data.Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return false;

        _data.Books.Remove(book);
        // Also remove any historical loans for this book
        _data.Loans.RemoveAll(l => l.BookId == id);
        return true;
    }

    // --- Loan operations ---

    public Loan BorrowBook(Guid memberId, Guid bookId)
    {
        var book = _data.Books.FirstOrDefault(b => b.Id == bookId);
        if (book == null)
            throw new InvalidOperationException("Book not found.");
        if (!book.IsAvailable)
            throw new InvalidOperationException("Book is not available.");

        var member = _data.Users.FirstOrDefault(u => u.Id == memberId);
        if (member == null)
            throw new InvalidOperationException("Member not found.");

        book.IsAvailable = false;

        var loan = new Loan
        {
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = DateTime.Now,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            MemberName = member.FullName
        };
        _data.Loans.Add(loan);

        return loan;
    }

    public void ReturnBook(Guid loanId)
    {
        var loan = _data.Loans.FirstOrDefault(l => l.Id == loanId);
        if (loan == null)
            throw new InvalidOperationException("Loan not found.");

        loan.ReturnDate = DateTime.Now;

        var book = _data.Books.FirstOrDefault(b => b.Id == loan.BookId);
        if (book != null)
            book.IsAvailable = true;
    }

    public List<Loan> GetActiveLoansForMember(Guid memberId)
    {
        return _data.Loans
            .Where(l => l.MemberId == memberId && l.IsActive)
            .ToList();
    }

    public List<Loan> GetAllActiveLoans()
    {
        return _data.Loans.Where(l => l.IsActive).ToList();
    }

    public List<Loan> GetBorrowingHistory(Guid memberId)
    {
        return _data.Loans
            .Where(l => l.MemberId == memberId && !l.IsActive)
            .OrderByDescending(l => l.ReturnDate)
            .ToList();
    }
}
