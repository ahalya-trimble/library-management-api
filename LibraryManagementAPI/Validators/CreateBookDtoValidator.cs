using FluentValidation;
using LibraryManagementAPI.DTOs;

namespace LibraryManagementAPI.Validators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(150).WithMessage("Author must not exceed 150 characters.");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Matches(@"^(\d{10}|\d{13})$").WithMessage("ISBN must be either 10 or 13 digits.");
    }
}
