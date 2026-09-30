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
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers([FromQuery] MemberQueryParameters queryParameters)
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

        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Member>> GetMemberById(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<Member>> GetMemberByEmail([FromQuery] string email)
    {
        var member = await _memberRepository.GetMemberByEmailAsync(email.Trim());
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(Member member)
    {
        var email = member.Email.Trim();
        if (await _memberRepository.EmailExistsAsync(email))
            return Conflict("A member with this email already exists.");

        member.Name = member.Name.Trim();
        member.Email = email;
        await _memberRepository.AddAsync(member);

        return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, member);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMember(int id, Member updatedMember)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member is null) return NotFound();

        var email = updatedMember.Email.Trim();
        if (await _memberRepository.EmailExistsAsync(email, id))
            return Conflict("A member with this email already exists.");

        member.Name = updatedMember.Name.Trim();
        member.Email = email;
        member.MembershipDate = updatedMember.MembershipDate;
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
}
