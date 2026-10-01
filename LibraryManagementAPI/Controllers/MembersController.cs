using System.Text.Json;
using LibraryManagementAPI.Authorization;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;

    public MembersController(IMemberRepository memberRepository) => _memberRepository = memberRepository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers([FromQuery] MemberQueryParameters queryParameters)
    {
        var result = await _memberRepository.GetMembersAsync(queryParameters);

        var metadata = new
        {
            result.TotalCount,
            result.PageSize,
            result.PageNumber,
            result.TotalPages
        };

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

        return Ok(result.Items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDto>> GetMemberById(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(ToDto(member));
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<MemberDto>> GetMemberByEmail([FromQuery] string email)
    {
        var member = await _memberRepository.GetMemberByEmailAsync(email.Trim());
        return member is null ? NotFound() : Ok(ToDto(member));
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> CreateMember(CreateMemberDto dto)
    {
        var email = dto.Email.Trim();
        if (await _memberRepository.EmailExistsAsync(email))
            return Conflict("A member with this email already exists.");

        var member = new Member
        {
            Name = dto.Name.Trim(),
            Email = email,
            MembershipDate = dto.MembershipDate
        };

        await _memberRepository.AddAsync(member);

        return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, ToDto(member));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMember(int id, UpdateMemberDto dto)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member is null) return NotFound();

        var email = dto.Email.Trim();
        if (await _memberRepository.EmailExistsAsync(email, id))
            return Conflict("A member with this email already exists.");

        member.Name = dto.Name.Trim();
        member.Email = email;
        member.MembershipDate = dto.MembershipDate;
        await _memberRepository.UpdateAsync(member);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member is null) return NotFound();

        await _memberRepository.DeleteAsync(member);
        return NoContent();
    }

    private static MemberDto ToDto(Member member) => new()
    {
        Id = member.Id,
        Name = member.Name,
        Email = member.Email,
        MembershipDate = member.MembershipDate
    };
}
