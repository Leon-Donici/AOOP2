# Library Management App - Project Report

## Project Overview

A Library Management Application built in C# using the Avalonia UI framework with MVVM architecture. The app supports Member and Librarian roles with login-based access control, book catalog management, borrowing/returning books, and JSON-based data persistence.

## Team Member Contributions

### Team Member 1 — Project Setup & Models
**Responsibilities:**
- Created the solution structure and project scaffolding
- Configured Avalonia project with required NuGet packages (Avalonia 11.3.12, CommunityToolkit.Mvvm, Avalonia.Controls.DataGrid)
- Designed and implemented data models:
  - `Book.cs` — Book entity with Id, Title, Author, ISBN, Description, IsAvailable
  - `User.cs` — User entity with Id, Username, PasswordHash, FullName, Role
  - `Loan.cs` — Loan entity tracking borrow/return with denormalized book/member info
  - `LibraryData.cs` — Root data container holding all Books, Users, and Loans
- Created `.gitignore` and project configuration

**Files:** `LibraryManagementApp.csproj`, `Models/Book.cs`, `Models/User.cs`, `Models/Loan.cs`, `Models/LibraryData.cs`, `.gitignore`

---

### Team Member 2 — Services (Business Logic & Persistence)
**Responsibilities:**
- Implemented all business logic in the service layer:
  - `LibraryService.cs` — Core service handling authentication, book CRUD operations, borrowing/returning, loan tracking, and borrowing history
  - `DataService.cs` — JSON persistence using System.Text.Json, with seed data generation for initial setup
  - `PasswordHasher.cs` — SHA-256 password hashing for secure credential storage (bonus feature)
- Implemented seed data: 2 default users (member/password, admin/password) and 5 classic books
- Handled edge cases: preventing deletion of books with active loans, preventing double-borrowing

**Files:** `Services/LibraryService.cs`, `Services/DataService.cs`, `Services/PasswordHasher.cs`

---

### Team Member 3 — ViewModels (MVVM Logic)
**Responsibilities:**
- Implemented all ViewModel classes following the MVVM pattern using CommunityToolkit.Mvvm:
  - `ViewModelBase.cs` — Base class extending ObservableObject
  - `MainWindowViewModel.cs` — Navigation hub managing view switching, save/load commands, login/logout flow
  - `LoginViewModel.cs` — Login form logic with validation and error messages
  - `MemberViewModel.cs` — Member dashboard logic: catalog browsing, search, borrowing, returning, history
  - `LibrarianViewModel.cs` — Librarian dashboard logic: book CRUD form, catalog management, active loan tracking
- Designed the navigation pattern using callback-based view switching
- Used `[ObservableProperty]` and `[RelayCommand]` attributes for reactive bindings

**Files:** `ViewModels/ViewModelBase.cs`, `ViewModels/MainWindowViewModel.cs`, `ViewModels/LoginViewModel.cs`, `ViewModels/MemberViewModel.cs`, `ViewModels/LibrarianViewModel.cs`

---

### Team Member 4 — Views (UI), App Wiring & Testing
**Responsibilities:**
- Designed and implemented all Avalonia AXAML views:
  - `MainWindow.axaml` — Application shell with top bar (Save/Load), status bar, and content area
  - `LoginView.axaml` — Centered login card with username, password, and error display
  - `MemberView.axaml` — TabControl with Library Catalog (search + book details + borrow) and My Loans (active loans + return + borrowing history)
  - `LibrarianView.axaml` — TabControl with Manage Catalog (book list + add/edit/delete form) and Active Loans (DataGrid + total count)
- Implemented `ViewLocator.cs` for convention-based ViewModel-to-View mapping
- Wired up `App.axaml.cs` as the composition root (service creation, data loading, shutdown save)
- Wrote all tests:
  - `LibraryServiceTests.cs` — 14 unit tests covering auth, book CRUD, loan operations
  - `DataServiceTests.cs` — 3 tests for JSON persistence
  - `HeadlessTests.cs` — 5 Avalonia headless UI tests for login flow, view rendering, borrow/return
- Created `functional-tests.txt` with manual test results

**Files:** All `Views/*.axaml`, `Views/*.axaml.cs`, `ViewLocator.cs`, `App.axaml`, `App.axaml.cs`, `Program.cs`, all test files, `functional-tests.txt`

---

## SOLID Principles Applied

### Single Responsibility Principle (SRP)
Each class has one clear responsibility:
- `LibraryService` handles business logic (auth, books, loans)
- `DataService` handles only JSON serialization/deserialization
- `PasswordHasher` handles only password hashing
- Each ViewModel handles the logic for exactly one screen
- Each View handles only the visual presentation of one screen

### Open/Closed Principle (OCP)
- The `DataService` can be replaced with a different persistence mechanism (e.g., SQLite) without changing `LibraryService` or any ViewModel
- The `ViewLocator` maps ViewModels to Views by naming convention, so new views can be added without modifying existing code
- New user roles could be added by extending the `Role` field and creating new ViewModels/Views

### Liskov Substitution Principle (LSP)
- `ViewModelBase` serves as the common base for all ViewModels. Any ViewModel can be assigned to `MainWindowViewModel.CurrentView` and the `ContentControl` + `ViewLocator` will render the correct view
- All model classes can be serialized/deserialized uniformly through `LibraryData`

### Interface Segregation Principle (ISP)
- The service layer is kept minimal — `LibraryService` provides focused method groups (auth methods, book methods, loan methods) rather than one monolithic interface
- ViewModels only depend on the methods they actually use from the service

### Dependency Inversion Principle (DIP)
- ViewModels receive `LibraryService` through constructor injection rather than creating it themselves
- `MainWindowViewModel` receives all dependencies through its constructor
- `App.axaml.cs` acts as the composition root where all dependencies are wired together
- Navigation uses callback delegates (`Action<User>`, `Action`) rather than tight coupling between ViewModels

## Design Patterns Applied

### MVVM (Model-View-ViewModel)
The core architectural pattern. Models hold data, ViewModels expose data and commands via bindings, Views are pure AXAML with no code-behind logic. CommunityToolkit.Mvvm provides `ObservableObject`, `[ObservableProperty]`, and `[RelayCommand]` to minimize boilerplate.

### Observer Pattern
Built into the MVVM framework via `INotifyPropertyChanged` (through `ObservableObject`) and `ObservableCollection<T>`. When ViewModel properties change, the UI automatically updates. When collections are reassigned, bound ListBoxes/DataGrids refresh.

### Composition Root Pattern
`App.axaml.cs` serves as the single location where all services and ViewModels are instantiated and wired together. This keeps dependency creation centralized and easy to understand.

## Technology Stack
- **Language:** C# (.NET 9)
- **UI Framework:** Avalonia 11.3.12 with Fluent theme
- **MVVM Toolkit:** CommunityToolkit.Mvvm 8.2.1
- **Persistence:** System.Text.Json (built into .NET)
- **Testing:** xUnit + Avalonia.Headless.XUnit
- **Bonus Features:** SHA-256 password hashing, Borrowing history

## How to Run
```bash
# Build
dotnet build

# Run the application
dotnet run --project LibraryManagementApp

# Run tests
dotnet test
```

## Default Login Credentials
- **Member:** username `member`, password `password`
- **Librarian:** username `admin`, password `password`
