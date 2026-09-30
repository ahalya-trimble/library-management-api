namespace LibraryManagementAPI.DTOs;

public class BookDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public bool Available { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public List<string> Categories { get; set; } = new();
}
