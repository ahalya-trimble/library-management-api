using FluentValidation;
using LibraryManagementAPI.DTOs;

namespace LibraryManagementAPI.Validators;

public class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewDtoValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0).WithMessage("A valid member id is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");
    }
}
