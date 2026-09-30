namespace LibraryManagementAPI.DTOs;

public class ReviewDto
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
