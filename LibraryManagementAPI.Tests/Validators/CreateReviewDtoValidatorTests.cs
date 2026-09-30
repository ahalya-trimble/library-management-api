using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Validators;
using Xunit;

namespace LibraryManagementAPI.Tests.Validators;

public class CreateReviewDtoValidatorTests
{
    private readonly CreateReviewDtoValidator _validator = new();

    private static CreateReviewDto ValidDto() => new()
    {
        MemberId = 1,
        Rating = 5,
        Comment = "Great book"
    };

    [Fact]
    public void Validate_ReturnsValid_ForValidDto()
    {
        var result = _validator.Validate(ValidDto());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenMemberIdIsZeroOrLess()
    {
        var dto = ValidDto();
        dto.MemberId = 0;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReviewDto.MemberId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_Fails_ForRatingOutOfRange(int rating)
    {
        var dto = ValidDto();
        dto.Rating = rating;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReviewDto.Rating));
    }

    [Fact]
    public void Validate_Fails_WhenCommentExceedsMaxLength()
    {
        var dto = ValidDto();
        dto.Comment = new string('a', 1001);

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReviewDto.Comment));
    }
}
