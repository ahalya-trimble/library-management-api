using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    Task<List<Category>> GetByIdsAsync(IEnumerable<int> ids);
}
