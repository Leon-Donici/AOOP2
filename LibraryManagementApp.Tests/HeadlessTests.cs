using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;
using LibraryManagementApp.ViewModels;
using LibraryManagementApp.Views;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(LibraryManagementApp.Tests.TestAppBuilder))]

namespace LibraryManagementApp.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public class HeadlessTests
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
                    Username = "member",
                    PasswordHash = PasswordHasher.Hash("password"),
                    FullName = "Test Member",
                    Role = "Member"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    PasswordHash = PasswordHasher.Hash("password"),
                    FullName = "Test Admin",
                    Role = "Librarian"
                }
            },
            Books = new()
            {
                new Book { Title = "Book One", Author = "Author A", IsAvailable = true },
                new Book { Title = "Book Two", Author = "Author B", IsAvailable = true }
            },
            Loans = new()
        };
        return (new LibraryService(data), data);
    }

    [AvaloniaFact]
    public void LoginView_ShouldRenderLoginButton()
    {
        var (service, _) = CreateTestService();
        var vm = new LoginViewModel(service, _ => { });
        var window = new Window
        {
            Content = new LoginView { DataContext = vm }
        };
        window.Show();

        // The login view should have rendered
        var loginView = window.Content as LoginView;
        Assert.NotNull(loginView);
    }

    [AvaloniaFact]
    public void LoginViewModel_InvalidLogin_SetsErrorMessage()
    {
        var (service, _) = CreateTestService();
        var vm = new LoginViewModel(service, _ => { });

        vm.Username = "wrong";
        vm.Password = "wrong";
        vm.LoginCommand.Execute(null);

        Assert.Equal("Invalid username or password.", vm.ErrorMessage);
    }

    [AvaloniaFact]
    public void LoginViewModel_ValidLogin_CallsCallback()
    {
        var (service, _) = CreateTestService();
        User? loggedInUser = null;
        var vm = new LoginViewModel(service, user => loggedInUser = user);

        vm.Username = "member";
        vm.Password = "password";
        vm.LoginCommand.Execute(null);

        Assert.NotNull(loggedInUser);
        Assert.Equal("Member", loggedInUser!.Role);
    }

    [AvaloniaFact]
    public void MemberView_ShouldDisplayBooks()
    {
        var (service, _) = CreateTestService();
        var member = service.Login("member", "password")!;
        var vm = new MemberViewModel(service, member, () => { });
        var window = new Window
        {
            Content = new MemberView { DataContext = vm }
        };
        window.Show();

        // VM should have loaded available books
        Assert.Equal(2, vm.AvailableBooks.Count);
    }

    [AvaloniaFact]
    public void MemberViewModel_BorrowAndReturn_WorksCorrectly()
    {
        var (service, data) = CreateTestService();
        var member = service.Login("member", "password")!;
        var vm = new MemberViewModel(service, member, () => { });

        // Select and borrow a book
        vm.SelectedBook = vm.AvailableBooks[0];
        vm.BorrowBookCommand.Execute(null);

        Assert.Single(vm.ActiveLoans);
        Assert.Single(vm.AvailableBooks); // one less available

        // Return the book
        vm.ReturnBookCommand.Execute(vm.ActiveLoans[0]);

        Assert.Empty(vm.ActiveLoans);
        Assert.Equal(2, vm.AvailableBooks.Count);
        Assert.Single(vm.BorrowingHistory);
    }
}
