using FluentValidation;
using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.Validators;

public class MemberValidator : AbstractValidator<Member>
{
    public MemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.MembershipDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Membership date cannot be in the future.");
    }
}
