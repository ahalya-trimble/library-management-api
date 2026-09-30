using LibraryManagementAPI.Authorization;
using LibraryManagementAPI.Controllers;
using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LibraryManagementAPI.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly LibraryDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!);

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new LibraryDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_TEST_ONLY_SECRET_KEY_1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "30"
            })
            .Build();
        _jwtTokenService = new JwtTokenService(configuration);

        _controller = new AuthController(_userManagerMock.Object, _signInManagerMock.Object, _dbContext, _jwtTokenService);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com"))
            .ReturnsAsync(new ApplicationUser { Email = "john@example.com" });

        var result = await _controller.Register(new RegisterDto { FullName = "John", Email = "john@example.com", Password = "Password123!" });

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ReturnsOk_WithToken_WhenSuccessful()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com")).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Member))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Register(new RegisterDto { FullName = "John", Email = "john@example.com", Password = "Password123!" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("john@example.com", response.Email);
        Assert.Contains(Roles.Member, response.Roles);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenCreateFails()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com")).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        var result = await _controller.Register(new RegisterDto { FullName = "John", Email = "john@example.com", Password = "weak" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var result = await _controller.Login(new LoginDto { Email = "missing@example.com", Password = "Password123!" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        var user = new ApplicationUser { Email = "john@example.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com")).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, "WrongPassword", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await _controller.Login(new LoginDto { Email = "john@example.com", Password = "WrongPassword" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithToken_WhenCredentialsAreValid()
    {
        var user = new ApplicationUser { Email = "john@example.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com")).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { Roles.Member });

        var result = await _controller.Login(new LoginDto { Email = "john@example.com", Password = "Password123!" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Contains(Roles.Member, response.Roles);
    }

    [Fact]
    public async Task AssignRole_ReturnsBadRequest_WhenRoleIsInvalid()
    {
        var result = await _controller.AssignRole(new AssignRoleDto { Email = "john@example.com", Role = "NotARole" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AssignRole_ReturnsNotFound_WhenUserMissing()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var result = await _controller.AssignRole(new AssignRoleDto { Email = "missing@example.com", Role = Roles.Librarian });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AssignRole_ReturnsBadRequest_WhenAlreadyInRole()
    {
        var user = new ApplicationUser { Email = "john@example.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsInRoleAsync(user, Roles.Librarian)).ReturnsAsync(true);

        var result = await _controller.AssignRole(new AssignRoleDto { Email = "john@example.com", Role = Roles.Librarian });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AssignRole_ReturnsOk_WhenSuccessful()
    {
        var user = new ApplicationUser { Email = "john@example.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@example.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsInRoleAsync(user, Roles.Librarian)).ReturnsAsync(false);
        _userManagerMock.Setup(m => m.AddToRoleAsync(user, Roles.Librarian)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.AssignRole(new AssignRoleDto { Email = "john@example.com", Role = Roles.Librarian });

        Assert.IsType<OkObjectResult>(result);
    }
}
