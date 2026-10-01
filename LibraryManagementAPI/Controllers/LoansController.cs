
using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly LibraryDbContext _context;

    public LoansController(LibraryDbContext context)
    {
        _context = context;
    }

    // 1. GET: api/loans
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
    {
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .ToListAsync();

        return Ok(loans.Select(ToDto));
    }

    // 2. GET: api/loans/1
    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetLoanById(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
            return NotFound();

        return ToDto(loan);
    }

    // 3. POST: api/loans
    [HttpPost]
    public async Task<ActionResult<LoanDto>> CreateLoan(CreateLoanDto dto)
    {
        if (dto.DueDate <= dto.LoanDate)
            return BadRequest("Due date must be after the loan date.");

        var book = await _context.Books.FindAsync(dto.BookId);

        if (book == null)
            return BadRequest("The specified book does not exist.");

        var member = await _context.Members.FindAsync(dto.MemberId);

        if (member == null)
            return BadRequest("The specified member does not exist.");

        if (!book.Available)
            return Conflict("This book is already on loan.");

        var loan = new Loan
        {
            BookId = dto.BookId,
            MemberId = dto.MemberId,
            LoanDate = dto.LoanDate,
            DueDate = dto.DueDate
        };

        book.Available = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        loan.Book = book;
        loan.Member = member;

        return CreatedAtAction(
            nameof(GetLoanById),
            new { id = loan.Id },
            ToDto(loan));
    }

    // 4. PUT: api/loans/1
    // Update the due date without changing the loan history.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLoan(
        int id, UpdateLoanDto dto)
    {
        var loan = await _context.Loans.FindAsync(id);

        if (loan == null)
            return NotFound();

        if (loan.ReturnDate != null)
            return Conflict("A returned loan cannot be modified.");

        if (dto.DueDate <= loan.LoanDate)
            return BadRequest("Due date must be after the loan date.");

        loan.DueDate = dto.DueDate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 5. POST: api/loans/1/return
    [HttpPost("{id}/return")]
    public async Task<IActionResult> ReturnLoan(int id)
    {
        var loan = await _context.Loans.FindAsync(id);

        if (loan == null)
            return NotFound("Loan not found.");

        if (loan.ReturnDate != null)
            return Conflict("This loan has already been returned.");

        var book = await _context.Books.FindAsync(loan.BookId);

        if (book == null)
            return Conflict("The associated book was not found.");

        loan.ReturnDate = DateTime.UtcNow;
        book.Available = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 6. DELETE: api/loans/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var loan = await _context.Loans.FindAsync(id);

        if (loan == null)
            return NotFound();

        if (loan.ReturnDate == null)
            return Conflict(
                "Return the book before deleting its loan record.");

        _context.Loans.Remove(loan);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static LoanDto ToDto(Loan loan) => new()
    {
        Id = loan.Id,
        BookId = loan.BookId,
        BookTitle = loan.Book?.Title ?? string.Empty,
        MemberId = loan.MemberId,
        MemberName = loan.Member?.Name ?? string.Empty,
        LoanDate = loan.LoanDate,
        DueDate = loan.DueDate,
        ReturnDate = loan.ReturnDate,
        Fine = loan.Fine
    };
}