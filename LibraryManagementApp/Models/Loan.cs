using System;
using System.Text.Json.Serialization;

namespace LibraryManagementApp.Models;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime BorrowDate { get; set; } = DateTime.Now;
    public DateTime? ReturnDate { get; set; }

    // Denormalized fields for display purposes
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsActive => ReturnDate == null;
}
