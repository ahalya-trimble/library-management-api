using LibraryManagementAPI.Authorization;
using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly LibraryDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        LibraryDbContext dbContext,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return Conflict("A user with this email already exists.");

        var member = new Member
        {
            Name = dto.FullName,
            Email = dto.Email,
            MembershipDate = DateTime.UtcNow
        };
        _dbContext.Members.Add(member);
        await _dbContext.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            MemberId = member.Id
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            _dbContext.Members.Remove(member);
            await _dbContext.SaveChangesAsync();
            return BadRequest(createResult.Errors.Select(e => e.Description));
        }

        member.IdentityUserId = user.Id;
        await _dbContext.SaveChangesAsync();

        await _userManager.AddToRoleAsync(user, Roles.Member);

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user, new[] { Roles.Member });

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Email = user.Email!,
            Roles = new[] { Roles.Member }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Email = user.Email!,
            Roles = roles
        });
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> AssignRole(AssignRoleDto dto)
    {
        if (!Roles.All.Contains(dto.Role))
            return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", Roles.All)}.");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return NotFound("No user found with this email.");

        if (await _userManager.IsInRoleAsync(user, dto.Role))
            return BadRequest($"User is already in the '{dto.Role}' role.");

        var result = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok($"Role '{dto.Role}' assigned to '{dto.Email}'.");
    }
}
