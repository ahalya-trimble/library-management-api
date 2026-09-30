using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.Models;

public class Review
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public Book? Book { get; set; }

    public int MemberId { get; set; }

    public Member? Member { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
