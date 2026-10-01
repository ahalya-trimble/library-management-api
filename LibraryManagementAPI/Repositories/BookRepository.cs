using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Book>> GetAllAsync() =>
        await _dbSet.Include(b => b.Categories).Where(b => !b.IsDeleted).ToListAsync();

    public override async Task<Book?> GetByIdAsync(int id) =>
        await _dbSet.Include(b => b.Categories).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null) =>
        await _dbSet.AnyAsync(b => b.ISBN == isbn && (excludeId == null || b.Id != excludeId));

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync() =>
        await _dbSet.Include(b => b.Categories).Where(b => !b.IsDeleted && b.Available).ToListAsync();

    public async Task<PagedResult<Book>> GetBooksAsync(BookQueryParameters queryParameters)
    {
        var query = _dbSet.Include(b => b.Categories).Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(queryParameters.Author))
        {
            var author = queryParameters.Author.Trim();
            query = query.Where(b => EF.Functions.Like(b.Author, $"%{author}%"));
        }

        if (queryParameters.Available.HasValue)
        {
            query = query.Where(b => b.Available == queryParameters.Available.Value);
        }

        var isDescending = string.Equals(queryParameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = queryParameters.SortBy?.ToLowerInvariant() switch
        {
            "author" => isDescending ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
            "isbn" => isDescending ? query.OrderByDescending(b => b.ISBN) : query.OrderBy(b => b.ISBN),
            _ => isDescending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .ToListAsync();

        return new PagedResult<Book>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize
        };
    }
}
