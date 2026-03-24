using System;
using System.IO;
using System.Text.Json;
using LibraryManagementApp.Models;

namespace LibraryManagementApp.Services;

public class DataService
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DataService(string filePath = "library-data.json")
    {
        _filePath = filePath;
    }

    public LibraryData Load()
    {
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<LibraryData>(json, JsonOptions);
            if (data != null)
                return data;
        }

        // Create seed data if file doesn't exist
        var seedData = CreateSeedData();
        Save(seedData);
        return seedData;
    }

    public void Save(LibraryData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static LibraryData CreateSeedData()
    {
        var member = new User
        {
            Id = Guid.NewGuid(),
            Username = "member",
            PasswordHash = PasswordHasher.Hash("password"),
            FullName = "John Doe",
            Role = "Member"
        };

        var librarian = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = PasswordHasher.Hash("password"),
            FullName = "Jane Admin",
            Role = "Librarian"
        };

        var books = new[]
        {
            new Book
            {
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                Isbn = "978-0-7432-7356-5",
                Description = "A novel about the American Dream set in the Jazz Age.",
                IsAvailable = true
            },
            new Book
            {
                Title = "1984",
                Author = "George Orwell",
                Isbn = "978-0-452-28423-4",
                Description = "A dystopian novel about totalitarianism and surveillance.",
                IsAvailable = true
            },
            new Book
            {
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                Isbn = "978-0-06-112008-4",
                Description = "A story of racial injustice in the American South.",
                IsAvailable = true
            },
            new Book
            {
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                Isbn = "978-0-14-028329-7",
                Description = "A romantic novel about manners and marriage in Regency England.",
                IsAvailable = true
            },
            new Book
            {
                Title = "The Catcher in the Rye",
                Author = "J.D. Salinger",
                Isbn = "978-0-316-76948-0",
                Description = "A coming-of-age story about teenage alienation and rebellion.",
                IsAvailable = true
            }
        };

        return new LibraryData
        {
            Users = new(new[] { member, librarian }),
            Books = new(books),
            Loans = new()
        };
    }
}
