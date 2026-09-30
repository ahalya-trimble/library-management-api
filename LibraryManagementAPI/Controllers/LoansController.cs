
using LibraryManagementAPI.Data;
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
    public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
    {
        return await _context.Loans.ToListAsync();
    }

    // 2. GET: api/loans/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Loan>> GetLoanById(int id)
    {
        var loan = await _context.Loans.FindAsync(id);

        if (loan == null)
            return NotFound();

        return loan;
    }

    // 3. POST: api/loans
    [HttpPost]
    public async Task<ActionResult<Loan>> CreateLoan(Loan loan)
    {
        if (loan.DueDate <= loan.LoanDate)
            return BadRequest("Due date must be after the loan date.");

        var book = await _context.Books.FindAsync(loan.BookId);

        if (book == null)
            return BadRequest("The specified book does not exist.");

        var member = await _context.Members.FindAsync(loan.MemberId);

        if (member == null)
            return BadRequest("The specified member does not exist.");

        if (!book.Available)
            return Conflict("This book is already on loan.");

        if (loan.ReturnDate != null)
            return BadRequest("A new loan cannot already have a return date.");

        book.Available = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetLoanById),
            new { id = loan.Id },
            loan);
    }

    // 4. PUT: api/loans/1
    // Update the due date without changing the loan history.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLoan(
        int id, Loan updatedLoan)
    {
        var loan = await _context.Loans.FindAsync(id);

        if (loan == null)
            return NotFound();

        if (loan.ReturnDate != null)
            return Conflict("A returned loan cannot be modified.");

        if (updatedLoan.DueDate <= loan.LoanDate)
            return BadRequest("Due date must be after the loan date.");

        loan.DueDate = updatedLoan.DueDate;

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
}