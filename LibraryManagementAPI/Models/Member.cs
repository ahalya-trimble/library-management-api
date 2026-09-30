using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "A valid email address is required.")]
        public string Email { get; set; } = string.Empty;

        public DateTime MembershipDate { get; set; }

        /// <summary>
        /// The Id of the corresponding ASP.NET Core Identity user (AspNetUsers.Id)
        /// managed by the separate Identity project. Null for members without a login.
        /// </summary>
        public string? IdentityUserId { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
