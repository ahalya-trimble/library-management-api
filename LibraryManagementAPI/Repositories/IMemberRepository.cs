using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    Task<Member?> GetMemberByEmailAsync(string email);

    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    Task<PagedResult<Member>> GetMembersAsync(MemberQueryParameters queryParameters);
}
