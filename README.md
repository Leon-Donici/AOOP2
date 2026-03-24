# Library Management App

A desktop application for managing a library's book collection and loan system, built with C# and Avalonia using the MVVM pattern.

## Features

- **Login system** with Member and Librarian roles
- **Member functionality**: browse catalog, search books, borrow/return books, view borrowing history
- **Librarian functionality**: add/edit/delete books, view all active loans with borrower info
- **Data persistence** via JSON file (auto-saves on exit, manual save/load buttons)
- **Password hashing** with SHA-256
- **Borrowing history** tracking for returned books

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run

```bash
dotnet run --project LibraryManagementApp
```

### Test

```bash
dotnet test
```

## Default Accounts

| Role | Username | Password |
|------|----------|----------|
| Member | `member` | `password` |
| Librarian | `admin` | `password` |

## Project Structure

```
LibraryManagementApp/
├── Models/          Book, User, Loan, LibraryData
├── Services/        LibraryService, DataService, PasswordHasher
├── ViewModels/      MainWindowVM, LoginVM, MemberVM, LibrarianVM
└── Views/           MainWindow, LoginView, MemberView, LibrarianView

LibraryManagementApp.Tests/
├── LibraryServiceTests.cs    14 unit tests
├── DataServiceTests.cs       3 persistence tests
└── HeadlessTests.cs          5 Avalonia headless UI tests
```

## Tech Stack

- **C# / .NET 9**
- **Avalonia 11.3** with Fluent theme
- **CommunityToolkit.Mvvm** for MVVM bindings
- **System.Text.Json** for persistence
- **xUnit + Avalonia.Headless.XUnit** for testing
