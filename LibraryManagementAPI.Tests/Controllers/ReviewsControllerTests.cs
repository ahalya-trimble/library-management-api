using LibraryManagementAPI.Controllers;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryManagementAPI.Tests.Controllers;

public class ReviewsControllerTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IMemberRepository> _memberRepositoryMock = new();
    private readonly ReviewsController _controller;

    public ReviewsControllerTests()
    {
        _controller = new ReviewsController(_reviewRepositoryMock.Object, _bookRepositoryMock.Object, _memberRepositoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetReviewsForBook_ReturnsNotFound_WhenBookMissing()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book?)null);

        var result = await _controller.GetReviewsForBook(1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReviewsForBook_ReturnsOk_WithMappedDtos()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1, Title = "Title" });
        _reviewRepositoryMock.Setup(r => r.GetByBookIdAsync(1)).ReturnsAsync(new List<Review>
        {
            new() { Id = 1, BookId = 1, MemberId = 1, Rating = 5, Comment = "Great" }
        });

        var result = await _controller.GetReviewsForBook(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(okResult.Value);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task CreateReview_ReturnsNotFound_WhenBookMissing()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book?)null);

        var result = await _controller.CreateReview(1, new CreateReviewDto { MemberId = 1, Rating = 5, Comment = "Great" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateReview_ReturnsNotFound_WhenMemberMissing()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1 });
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Member?)null);

        var result = await _controller.CreateReview(1, new CreateReviewDto { MemberId = 1, Rating = 5, Comment = "Great" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateReview_ReturnsBadRequest_WhenMemberHasNotBorrowedBook()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1 });
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, Name = "John", Email = "john@example.com" });
        _reviewRepositoryMock.Setup(r => r.HasMemberBorrowedBookAsync(1, 1)).ReturnsAsync(false);

        var result = await _controller.CreateReview(1, new CreateReviewDto { MemberId = 1, Rating = 5, Comment = "Great" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateReview_ReturnsConflict_WhenAlreadyReviewed()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1 });
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, Name = "John", Email = "john@example.com" });
        _reviewRepositoryMock.Setup(r => r.HasMemberBorrowedBookAsync(1, 1)).ReturnsAsync(true);
        _reviewRepositoryMock.Setup(r => r.HasMemberReviewedBookAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.CreateReview(1, new CreateReviewDto { MemberId = 1, Rating = 5, Comment = "Great" });

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateReview_ReturnsCreatedAtAction_WhenValid()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1 });
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, Name = "John", Email = "john@example.com" });
        _reviewRepositoryMock.Setup(r => r.HasMemberBorrowedBookAsync(1, 1)).ReturnsAsync(true);
        _reviewRepositoryMock.Setup(r => r.HasMemberReviewedBookAsync(1, 1)).ReturnsAsync(false);

        var result = await _controller.CreateReview(1, new CreateReviewDto { MemberId = 1, Rating = 5, Comment = "Great" });

        Assert.IsType<CreatedAtActionResult>(result.Result);
        _reviewRepositoryMock.Verify(r => r.AddAsync(It.Is<Review>(rv => rv.BookId == 1 && rv.MemberId == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateReview_ReturnsNotFound_WhenMissing()
    {
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Review?)null);

        var result = await _controller.UpdateReview(1, new UpdateReviewDto { MemberId = 1, Rating = 4, Comment = "Good" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateReview_ReturnsForbidden_WhenNotOwnReview()
    {
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, MemberId = 2 });

        var result = await _controller.UpdateReview(1, new UpdateReviewDto { MemberId = 1, Rating = 4, Comment = "Good" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateReview_ReturnsNoContent_WhenOwnReview()
    {
        var review = new Review { Id = 1, MemberId = 1, Rating = 2, Comment = "Old" };
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        var result = await _controller.UpdateReview(1, new UpdateReviewDto { MemberId = 1, Rating = 4, Comment = "Good" });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(4, review.Rating);
        _reviewRepositoryMock.Verify(r => r.UpdateAsync(review), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_ReturnsForbidden_WhenNotOwnReview()
    {
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, MemberId = 2 });

        var result = await _controller.DeleteReview(1, memberId: 1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteReview_ReturnsNoContent_WhenOwnReview()
    {
        var review = new Review { Id = 1, MemberId = 1 };
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        var result = await _controller.DeleteReview(1, memberId: 1);

        Assert.IsType<NoContentResult>(result);
        _reviewRepositoryMock.Verify(r => r.DeleteAsync(review), Times.Once);
    }
}
