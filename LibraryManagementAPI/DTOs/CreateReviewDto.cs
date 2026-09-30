using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.DTOs;

public class CreateReviewDto
{
    [Required]
    public int MemberId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;
}
