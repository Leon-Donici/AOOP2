using System;
using System.IO;
using LibraryManagementApp.Models;
using LibraryManagementApp.Services;
using Xunit;

namespace LibraryManagementApp.Tests;

public class DataServiceTests : IDisposable
{
    private readonly string _testFilePath;

    public DataServiceTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"library-test-{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public void Load_NoFile_CreatesSeedData()
    {
        var service = new DataService(_testFilePath);

        var data = service.Load();

        Assert.NotNull(data);
        Assert.True(data.Books.Count > 0);
        Assert.True(data.Users.Count >= 2); // at least member + librarian
        Assert.Contains(data.Users, u => u.Role == "Member");
        Assert.Contains(data.Users, u => u.Role == "Librarian");
    }

    [Fact]
    public void SaveAndLoad_PersistsData()
    {
        var service = new DataService(_testFilePath);
        var data = new LibraryData
        {
            Books = new() { new Book { Title = "Persisted Book", Author = "Author X" } },
            Users = new() { new User { Username = "user1", FullName = "User One", Role = "Member" } },
            Loans = new()
        };

        service.Save(data);
        var loaded = service.Load();

        Assert.Single(loaded.Books);
        Assert.Equal("Persisted Book", loaded.Books[0].Title);
        Assert.Single(loaded.Users);
        Assert.Equal("user1", loaded.Users[0].Username);
    }

    [Fact]
    public void Save_CreatesJsonFile()
    {
        var service = new DataService(_testFilePath);
        var data = new LibraryData();

        service.Save(data);

        Assert.True(File.Exists(_testFilePath));
        var content = File.ReadAllText(_testFilePath);
        Assert.Contains("books", content);
    }
}
