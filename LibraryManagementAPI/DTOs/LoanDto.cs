namespace LibraryManagementAPI.DTOs;

public class LoanDto
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public string BookTitle { get; set; } = string.Empty;

    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;

    public DateTime LoanDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public decimal Fine { get; set; }
}
