using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Validators;
using Xunit;

namespace LibraryManagementAPI.Tests.Validators;

public class CreateBookDtoValidatorTests
{
    private readonly CreateBookDtoValidator _validator = new();

    private static CreateBookDto ValidDto() => new()
    {
        Title = "Valid Title",
        Author = "Valid Author",
        ISBN = "1234567890"
    };

    [Fact]
    public void Validate_ReturnsValid_ForValidDto()
    {
        var result = _validator.Validate(ValidDto());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenTitleIsEmpty()
    {
        var dto = ValidDto();
        dto.Title = string.Empty;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookDto.Title));
    }

    [Fact]
    public void Validate_Fails_WhenAuthorIsEmpty()
    {
        var dto = ValidDto();
        dto.Author = string.Empty;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookDto.Author));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    public void Validate_Fails_ForInvalidIsbn(string isbn)
    {
        var dto = ValidDto();
        dto.ISBN = isbn;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookDto.ISBN));
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("1234567890123")]
    public void Validate_Succeeds_ForValidIsbnLengths(string isbn)
    {
        var dto = ValidDto();
        dto.ISBN = isbn;

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenTitleExceedsMaxLength()
    {
        var dto = ValidDto();
        dto.Title = new string('a', 201);

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookDto.Title));
    }
}
