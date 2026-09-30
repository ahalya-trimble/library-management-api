using LibraryManagementAPI.Data;
using LibraryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(LibraryDbContext context) : base(context)
    {
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<Category>> GetByIdsAsync(IEnumerable<int> ids) =>
        await _dbSet.Where(c => ids.Contains(c.Id)).ToListAsync();
}
