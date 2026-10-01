using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.DTOs;

public class CreateMemberDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;

    public DateTime MembershipDate { get; set; }
}
