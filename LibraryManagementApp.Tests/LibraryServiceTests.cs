using System;
using System.Linq;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;
using Xunit;

namespace LibraryManagementApp.Tests;

public class LibraryServiceTests
{
    private static (LibraryService service, LibraryData data) CreateTestService()
    {
        var data = new LibraryData
        {
            Users = new()
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testmember",
                    PasswordHash = PasswordHasher.Hash("pass123"),
                    FullName = "Test Member",
                    Role = "Member"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testadmin",
                    PasswordHash = PasswordHasher.Hash("admin123"),
                    FullName = "Test Admin",
                    Role = "Librarian"
                }
            },
            Books = new()
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 1",
                    Author = "Author A",
                    Isbn = "111-1111111111",
                    Description = "A test book",
                    IsAvailable = true
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 2",
                    Author = "Author B",
                    Isbn = "222-2222222222",
                    Description = "Another test book",
                    IsAvailable = true
                }
            },
            Loans = new()
        };
        return (new LibraryService(data), data);
    }

    // --- Authentication Tests ---

    [Fact]
    public void Login_ValidMemberCredentials_ReturnsUser()
    {
        var (service, _) = CreateTestService();

        var user = service.Login("testmember", "pass123");

        Assert.NotNull(user);
        Assert.Equal("Member", user.Role);
        Assert.Equal("Test Member", user.FullName);
    }

    [Fact]
    public void Login_ValidLibrarianCredentials_ReturnsUser()
    {
        var (service, _) = CreateTestService();

        var user = service.Login("testadmin", "admin123");

        Assert.NotNull(user);
        Assert.Equal("Librarian", user.Role);
    }

    [Fact]
    public void Login_InvalidPassword_ReturnsNull()
    {
        var (service, _) = CreateTestService();

        var user = service.Login("testmember", "wrongpassword");

        Assert.Null(user);
    }

    [Fact]
    public void Login_NonexistentUser_ReturnsNull()
    {
        var (service, _) = CreateTestService();

        var user = service.Login("nobody", "pass123");

        Assert.Null(user);
    }

    // --- Book Operations Tests ---

    [Fact]
    public void SearchBooks_ByTitle_ReturnsMatches()
    {
        var (service, _) = CreateTestService();

        var results = service.SearchBooks("Test Book 1");

        Assert.Single(results);
        Assert.Equal("Test Book 1", results[0].Title);
    }

    [Fact]
    public void SearchBooks_ByAuthor_CaseInsensitive()
    {
        var (service, _) = CreateTestService();

        var results = service.SearchBooks("author a");

        Assert.Single(results);
        Assert.Equal("Author A", results[0].Author);
    }

    [Fact]
    public void AddBook_IncreasesBookCount()
    {
        var (service, data) = CreateTestService();
        var countBefore = data.Books.Count;

        service.AddBook("New Book", "New Author", "333-3333", "New description");

        Assert.Equal(countBefore + 1, data.Books.Count);
        Assert.Contains(data.Books, b => b.Title == "New Book");
    }

    [Fact]
    public void DeleteBook_RemovesBook()
    {
        var (service, data) = CreateTestService();
        var bookId = data.Books[0].Id;

        var result = service.DeleteBook(bookId);

        Assert.True(result);
        Assert.DoesNotContain(data.Books, b => b.Id == bookId);
    }

    [Fact]
    public void DeleteBook_WithActiveLoan_ReturnsFalse()
    {
        var (service, data) = CreateTestService();
        var memberId = data.Users[0].Id;
        var bookId = data.Books[0].Id;

        // Borrow the book first
        service.BorrowBook(memberId, bookId);

        // Try to delete — should fail
        var result = service.DeleteBook(bookId);
        Assert.False(result);
    }

    // --- Loan Operations Tests ---

    [Fact]
    public void BorrowBook_MakesBookUnavailable()
    {
        var (service, data) = CreateTestService();
        var memberId = data.Users[0].Id;
        var bookId = data.Books[0].Id;

        var loan = service.BorrowBook(memberId, bookId);

        Assert.NotNull(loan);
        Assert.False(data.Books.First(b => b.Id == bookId).IsAvailable);
        Assert.Single(service.GetActiveLoansForMember(memberId));
    }

    [Fact]
    public void BorrowBook_UnavailableBook_ThrowsException()
    {
        var (service, data) = CreateTestService();
        var memberId = data.Users[0].Id;
        var bookId = data.Books[0].Id;

        // Borrow it once
        service.BorrowBook(memberId, bookId);

        // Try to borrow again
        Assert.Throws<InvalidOperationException>(() =>
            service.BorrowBook(memberId, bookId));
    }

    [Fact]
    public void ReturnBook_MakesBookAvailable()
    {
        var (service, data) = CreateTestService();
        var memberId = data.Users[0].Id;
        var bookId = data.Books[0].Id;

        var loan = service.BorrowBook(memberId, bookId);
        service.ReturnBook(loan.Id);

        Assert.True(data.Books.First(b => b.Id == bookId).IsAvailable);
        Assert.Empty(service.GetActiveLoansForMember(memberId));
    }

    [Fact]
    public void ReturnBook_CreatesHistoryRecord()
    {
        var (service, data) = CreateTestService();
        var memberId = data.Users[0].Id;
        var bookId = data.Books[0].Id;

        var loan = service.BorrowBook(memberId, bookId);
        service.ReturnBook(loan.Id);

        var history = service.GetBorrowingHistory(memberId);
        Assert.Single(history);
        Assert.Equal(bookId, history[0].BookId);
        Assert.NotNull(history[0].ReturnDate);
    }

    [Fact]
    public void GetAllActiveLoans_ReturnsOnlyActiveLoans()
    {
        var (service, data) = CreateTestService();
        var memberId = data.Users[0].Id;

        var loan1 = service.BorrowBook(memberId, data.Books[0].Id);
        service.BorrowBook(memberId, data.Books[1].Id);
        service.ReturnBook(loan1.Id);

        var activeLoans = service.GetAllActiveLoans();
        Assert.Single(activeLoans);
        Assert.Equal(data.Books[1].Id, activeLoans[0].BookId);
    }
}
