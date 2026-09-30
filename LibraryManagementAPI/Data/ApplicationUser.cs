using Microsoft.AspNetCore.Identity;

namespace LibraryManagementAPI.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The Id of the corresponding Member record.
    /// </summary>
    public int MemberId { get; set; }
}
