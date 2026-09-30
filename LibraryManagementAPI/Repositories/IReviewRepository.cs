using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByBookIdAsync(int bookId);

    Task<bool> HasMemberBorrowedBookAsync(int memberId, int bookId);

    Task<bool> HasMemberReviewedBookAsync(int memberId, int bookId);
}
