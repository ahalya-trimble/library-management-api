using LibraryManagementAPI.Controllers;
using LibraryManagementAPI.Data;
using LibraryManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementAPI.Tests.Controllers;

public class LoansControllerTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    private static async Task<(Book book, Member member)> SeedBookAndMember(LibraryDbContext context, bool bookAvailable = true)
    {
        var book = new Book { Title = "Title", Author = "Author", ISBN = "1234567890", Available = bookAvailable };
        var member = new Member { Name = "John", Email = "john@example.com", MembershipDate = DateTime.UtcNow };
        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();
        return (book, member);
    }

    [Fact]
    public async Task CreateLoan_ReturnsBadRequest_WhenDueDateNotAfterLoanDate()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context);

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(-1) };

        var result = await controller.CreateLoan(loan);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateLoan_ReturnsConflict_WhenBookNotAvailable()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context, bookAvailable: false);

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(7) };

        var result = await controller.CreateLoan(loan);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateLoan_MarksBookUnavailable_AndReturnsCreatedAtAction()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context);

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(7) };

        var result = await controller.CreateLoan(loan);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.False(book.Available);
    }

    [Fact]
    public async Task ReturnLoan_ReturnsNotFound_WhenMissing()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);

        var result = await controller.ReturnLoan(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ReturnLoan_ReturnsConflict_WhenAlreadyReturned()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context, bookAvailable: false);
        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow, ReturnDate = DateTime.UtcNow };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var result = await controller.ReturnLoan(loan.Id);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task ReturnLoan_SetsReturnDate_AndMarksBookAvailable()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context, bookAvailable: false);
        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var result = await controller.ReturnLoan(loan.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.NotNull(loan.ReturnDate);
        Assert.True(book.Available);
    }

    [Fact]
    public async Task DeleteLoan_ReturnsConflict_WhenNotReturned()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context, bookAvailable: false);
        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var result = await controller.DeleteLoan(loan.Id);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task DeleteLoan_RemovesLoan_WhenReturned()
    {
        using var context = CreateContext();
        var controller = new LoansController(context);
        var (book, member) = await SeedBookAndMember(context);
        var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow.AddDays(-3), ReturnDate = DateTime.UtcNow };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var result = await controller.DeleteLoan(loan.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Loans.FindAsync(loan.Id));
    }
}
