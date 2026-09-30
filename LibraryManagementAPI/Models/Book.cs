namespace LibraryManagementAPI.Models;

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public bool Available { get; set; }

    public int TotalCopies { get; set; } = 1;

    public int AvailableCopies { get; set; } = 1;

    public bool IsDeleted { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}