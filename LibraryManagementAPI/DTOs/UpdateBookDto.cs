using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.DTOs;

public class UpdateBookDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(\d{10}|\d{13})$", ErrorMessage = "ISBN must be either 10 or 13 digits.")]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    public bool Available { get; set; }

    public int TotalCopies { get; set; } = 1;

    public List<int> CategoryIds { get; set; } = new();
}
