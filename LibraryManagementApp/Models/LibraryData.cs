using System.Collections.Generic;

namespace LibraryManagementApp.Models;

public class LibraryData
{
    public List<Book> Books { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<Loan> Loans { get; set; } = new();
}
