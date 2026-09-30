using LibraryManagementAPI.Models;
using LibraryManagementAPI.Validators;
using Xunit;

namespace LibraryManagementAPI.Tests.Validators;

public class MemberValidatorTests
{
    private readonly MemberValidator _validator = new();

    private static Member ValidMember() => new()
    {
        Name = "John Smith",
        Email = "john@example.com",
        MembershipDate = DateTime.UtcNow
    };

    [Fact]
    public void Validate_ReturnsValid_ForValidMember()
    {
        var result = _validator.Validate(ValidMember());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenNameIsEmpty()
    {
        var member = ValidMember();
        member.Name = string.Empty;

        var result = _validator.Validate(member);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Member.Name));
    }

    [Fact]
    public void Validate_Fails_WhenNameExceedsMaxLength()
    {
        var member = ValidMember();
        member.Name = new string('a', 101);

        var result = _validator.Validate(member);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Member.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_Fails_ForInvalidEmail(string email)
    {
        var member = ValidMember();
        member.Email = email;

        var result = _validator.Validate(member);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Member.Email));
    }

    [Fact]
    public void Validate_Fails_WhenMembershipDateIsInTheFuture()
    {
        var member = ValidMember();
        member.MembershipDate = DateTime.UtcNow.AddDays(1);

        var result = _validator.Validate(member);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Member.MembershipDate));
    }
}
