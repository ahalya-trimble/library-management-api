using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null);

    Task<IEnumerable<Book>> GetAvailableBooksAsync();

    Task<PagedResult<Book>> GetBooksAsync(BookQueryParameters queryParameters);
}
