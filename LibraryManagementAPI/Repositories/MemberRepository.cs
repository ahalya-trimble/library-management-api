using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Repositories;

public class MemberRepository : Repository<Member>, IMemberRepository
{
    public MemberRepository(LibraryDbContext context) : base(context)
    {
    }

    public async Task<Member?> GetMemberByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(m => m.Email == email);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
        await _dbSet.AnyAsync(m => m.Email == email && (excludeId == null || m.Id != excludeId));

    public async Task<PagedResult<Member>> GetMembersAsync(MemberQueryParameters queryParameters)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParameters.Name))
        {
            var name = queryParameters.Name.Trim();
            query = query.Where(m => EF.Functions.Like(m.Name, $"%{name}%"));
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.Email))
        {
            var email = queryParameters.Email.Trim();
            query = query.Where(m => EF.Functions.Like(m.Email, $"%{email}%"));
        }

        var isDescending = string.Equals(queryParameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = queryParameters.SortBy?.ToLowerInvariant() switch
        {
            "email" => isDescending ? query.OrderByDescending(m => m.Email) : query.OrderBy(m => m.Email),
            "membershipdate" => isDescending ? query.OrderByDescending(m => m.MembershipDate) : query.OrderBy(m => m.MembershipDate),
            _ => isDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .ToListAsync();

        return new PagedResult<Member>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize
        };
    }
}
