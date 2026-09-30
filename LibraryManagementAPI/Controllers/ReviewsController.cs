using LibraryManagementAPI.Authorization;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.Member},{Roles.Librarian},{Roles.Admin}")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;

    public ReviewsController(
        IReviewRepository reviewRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository)
    {
        _reviewRepository = reviewRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
    }

    [HttpGet("api/books/{bookId:int}/reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForBook(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) return NotFound("Book not found.");

        var reviews = await _reviewRepository.GetByBookIdAsync(bookId);
        return Ok(reviews.Select(ToDto));
    }

    [HttpPost("api/books/{bookId:int}/reviews")]
    public async Task<ActionResult<ReviewDto>> CreateReview(int bookId, CreateReviewDto dto)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null) return NotFound("Book not found.");

        var member = await _memberRepository.GetByIdAsync(dto.MemberId);
        if (member is null) return NotFound("Member not found.");

        if (!await _reviewRepository.HasMemberBorrowedBookAsync(dto.MemberId, bookId))
            return BadRequest("Members can only review books they have borrowed.");

        if (await _reviewRepository.HasMemberReviewedBookAsync(dto.MemberId, bookId))
            return Conflict("This member has already reviewed this book.");

        var review = new Review
        {
            BookId = bookId,
            MemberId = dto.MemberId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review);
        review.Member = member;

        return CreatedAtAction(nameof(GetReviewsForBook), new { bookId }, ToDto(review));
    }

    [HttpPut("api/reviews/{id:int}")]
    [Authorize(Roles = Roles.Member)]
    public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null) return NotFound();

        if (review.MemberId != dto.MemberId)
            return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own review.");

        review.Rating = dto.Rating;
        review.Comment = dto.Comment.Trim();

        await _reviewRepository.UpdateAsync(review);
        return NoContent();
    }

    [HttpDelete("api/reviews/{id:int}")]
    [Authorize(Roles = Roles.Member)]
    public async Task<IActionResult> DeleteReview(int id, [FromQuery] int memberId)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null) return NotFound();

        if (review.MemberId != memberId)
            return StatusCode(StatusCodes.Status403Forbidden, "You can only delete your own review.");

        await _reviewRepository.DeleteAsync(review);
        return NoContent();
    }

    private static ReviewDto ToDto(Review review) => new()
    {
        Id = review.Id,
        BookId = review.BookId,
        MemberId = review.MemberId,
        MemberName = review.Member?.Name ?? string.Empty,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedDate = review.CreatedDate
    };
}
