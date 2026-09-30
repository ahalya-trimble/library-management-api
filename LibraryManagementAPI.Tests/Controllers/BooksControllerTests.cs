using LibraryManagementAPI.Controllers;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryManagementAPI.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookRepository> _repositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Category>());

        _controller = new BooksController(_repositoryMock.Object, _categoryRepositoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetBooks_ReturnsOkWithMappedDtos()
    {
        var pagedResult = new PagedResult<Book>
        {
            Items = new List<Book>
            {
                new() { Id = 1, Title = "1984", ISBN = "9780451524935", Author = "George Orwell", Available = true }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _repositoryMock.Setup(r => r.GetBooksAsync(It.IsAny<BookQueryParameters>())).ReturnsAsync(pagedResult);

        var result = await _controller.GetBooks(new BookQueryParameters());

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetBookById_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book?)null);

        var result = await _controller.GetBookById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetBookById_ReturnsOk_WhenFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1, Title = "1984", Author = "George Orwell", ISBN = "9780451524935" });

        var result = await _controller.GetBookById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<BookDto>(okResult.Value);
        Assert.Equal("1984", dto.Title);
    }

    [Fact]
    public async Task CreateBook_ReturnsConflict_WhenIsbnExists()
    {
        _repositoryMock.Setup(r => r.IsbnExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(true);

        var result = await _controller.CreateBook(new CreateBookDto { Title = "Title", Author = "Author", ISBN = "1234567890" });

        Assert.IsType<ConflictObjectResult>(result.Result);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreatedAtAction_WhenIsbnIsUnique()
    {
        _repositoryMock.Setup(r => r.IsbnExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);

        var result = await _controller.CreateBook(new CreateBookDto { Title = "Title", Author = "Author", ISBN = "1234567890" });

        Assert.IsType<CreatedAtActionResult>(result.Result);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Book>(b => b.ISBN == "1234567890")), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book?)null);

        var result = await _controller.UpdateBook(1, new UpdateBookDto { Title = "Title", Author = "Author", ISBN = "1234567890" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateBook_ReturnsConflict_WhenIsbnExistsOnAnotherBook()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Book { Id = 1, Title = "Old", Author = "Old", ISBN = "0000000000" });
        _repositoryMock.Setup(r => r.IsbnExistsAsync("1234567890", 1)).ReturnsAsync(true);

        var result = await _controller.UpdateBook(1, new UpdateBookDto { Title = "Title", Author = "Author", ISBN = "1234567890" });

        Assert.IsType<ConflictObjectResult>(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBook_ReturnsNoContent_WhenUpdated()
    {
        var book = new Book { Id = 1, Title = "Old", Author = "Old", ISBN = "0000000000" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        _repositoryMock.Setup(r => r.IsbnExistsAsync("1234567890", 1)).ReturnsAsync(false);

        var result = await _controller.UpdateBook(1, new UpdateBookDto { Title = "New Title", Author = "New Author", ISBN = "1234567890" });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("New Title", book.Title);
        _repositoryMock.Verify(r => r.UpdateAsync(book), Times.Once);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book?)null);

        var result = await _controller.DeleteBook(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteBook_SoftDeletes_AndReturnsNoContent()
    {
        var book = new Book { Id = 1, Title = "Title", Author = "Author", ISBN = "1234567890", IsDeleted = false };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        var result = await _controller.DeleteBook(1);

        Assert.IsType<NoContentResult>(result);
        Assert.True(book.IsDeleted);
        _repositoryMock.Verify(r => r.UpdateAsync(book), Times.Once);
    }
}
