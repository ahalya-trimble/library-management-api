using LibraryManagementAPI.Data;
using LibraryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> GetByBookIdAsync(int bookId) =>
        await _dbSet
            .Include(r => r.Member)
            .Where(r => r.BookId == bookId)
            .ToListAsync();

    public async Task<bool> HasMemberBorrowedBookAsync(int memberId, int bookId) =>
        await _context.Loans.AnyAsync(l => l.MemberId == memberId && l.BookId == bookId);

    public async Task<bool> HasMemberReviewedBookAsync(int memberId, int bookId) =>
        await _dbSet.AnyAsync(r => r.MemberId == memberId && r.BookId == bookId);
}
