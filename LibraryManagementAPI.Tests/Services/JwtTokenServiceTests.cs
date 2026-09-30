using System.IdentityModel.Tokens.Jwt;
using LibraryManagementAPI.Data;
using LibraryManagementAPI.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LibraryManagementAPI.Tests.Services;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_TEST_ONLY_SECRET_KEY_1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "30"
            })
            .Build();

        return new JwtTokenService(configuration);
    }

    private static ApplicationUser CreateUser() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Email = "admin@example.com",
        FullName = "Admin User",
        MemberId = 1
    };

    [Fact]
    public void GenerateToken_ReturnsValidJwt_WithExpectedClaims()
    {
        var service = CreateService();
        var user = CreateUser();
        var roles = new List<string> { "Admin", "Member" };

        var (token, expiresAtUtc) = service.GenerateToken(user, roles);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAtUtc > DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains(jwt.Audiences, a => a == "TestAudience");
        Assert.Equal(user.Id, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(2, jwt.Claims.Count(c => c.Type == System.Security.Claims.ClaimTypes.Role));
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Member");
    }

    [Fact]
    public void GenerateToken_ExpiresAtUtc_RespectsConfiguredExpiryMinutes()
    {
        var service = CreateService();
        var user = CreateUser();

        var (_, expiresAtUtc) = service.GenerateToken(user, new List<string>());

        var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
        Assert.True(Math.Abs((expectedExpiry - expiresAtUtc).TotalSeconds) < 5);
    }

    [Fact]
    public void GenerateToken_Throws_WhenKeyIsNotConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var service = new JwtTokenService(configuration);

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(CreateUser(), new List<string>()));
    }
}
