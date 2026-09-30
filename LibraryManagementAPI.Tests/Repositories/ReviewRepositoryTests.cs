using LibraryManagementAPI.Data;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementAPI.Tests.Repositories;

public class ReviewRepositoryTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task GetByBookIdAsync_ReturnsOnlyReviewsForThatBook()
    {
        using var context = CreateContext();
        var repository = new ReviewRepository(context);
        var member = new Member { Name = "John", Email = "john@example.com" };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        await repository.AddAsync(new Review { BookId = 1, MemberId = member.Id, Rating = 5, Comment = "Great" });
        await repository.AddAsync(new Review { BookId = 2, MemberId = member.Id, Rating = 3, Comment = "Ok" });

        var result = await repository.GetByBookIdAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result.First().BookId);
        Assert.NotNull(result.First().Member);
    }

    [Fact]
    public async Task HasMemberBorrowedBookAsync_ReturnsTrue_WhenLoanExists()
    {
        using var context = CreateContext();
        var repository = new ReviewRepository(context);
        var member = new Member { Name = "John", Email = "john@example.com" };
        var book = new Book { Title = "Title", Author = "Author", ISBN = "1234567890" };
        context.Members.Add(member);
        context.Books.Add(book);
        await context.SaveChangesAsync();
        context.Loans.Add(new Loan { MemberId = member.Id, BookId = book.Id, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(7) });
        await context.SaveChangesAsync();

        var result = await repository.HasMemberBorrowedBookAsync(member.Id, book.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task HasMemberBorrowedBookAsync_ReturnsFalse_WhenNoLoanExists()
    {
        using var context = CreateContext();
        var repository = new ReviewRepository(context);

        var result = await repository.HasMemberBorrowedBookAsync(1, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task HasMemberReviewedBookAsync_ReturnsTrue_WhenReviewExists()
    {
        using var context = CreateContext();
        var repository = new ReviewRepository(context);
        var member = new Member { Name = "John", Email = "john@example.com" };
        context.Members.Add(member);
        await context.SaveChangesAsync();
        await repository.AddAsync(new Review { BookId = 1, MemberId = member.Id, Rating = 5, Comment = "Great" });

        var result = await repository.HasMemberReviewedBookAsync(member.Id, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task HasMemberReviewedBookAsync_ReturnsFalse_WhenNoReviewExists()
    {
        using var context = CreateContext();
        var repository = new ReviewRepository(context);

        var result = await repository.HasMemberReviewedBookAsync(1, 1);

        Assert.False(result);
    }
}
