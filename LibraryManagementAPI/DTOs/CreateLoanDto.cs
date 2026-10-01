namespace LibraryManagementAPI.DTOs;

public class CreateLoanDto
{
    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime DueDate { get; set; }
}
