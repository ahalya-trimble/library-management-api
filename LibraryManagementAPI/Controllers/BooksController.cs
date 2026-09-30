using System.Text.Json;
using LibraryManagementAPI.Authorization;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BooksController(IBookRepository bookRepository, ICategoryRepository categoryRepository)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks([FromQuery] BookQueryParameters queryParameters)
    {
        var result = await _bookRepository.GetBooksAsync(queryParameters);

        var metadata = new
        {
            result.TotalCount,
            result.PageSize,
            result.PageNumber,
            result.TotalPages
        };

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

        return Ok(result.Items.Select(ToDto));
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAvailableBooks()
    {
        var books = await _bookRepository.GetAvailableBooksAsync();
        return Ok(books.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetBookById(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(ToDto(book));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Librarian}")]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto dto)
    {
        var isbn = dto.ISBN.Trim();
        if (await _bookRepository.IsbnExistsAsync(isbn))
            return Conflict("A book with this ISBN already exists.");

        var book = new Book
        {
            Title = dto.Title.Trim(),
            ISBN = isbn,
            Author = dto.Author.Trim(),
            Available = dto.Available,
            IsDeleted = false
        };

        if (dto.CategoryIds.Count > 0)
        {
            book.Categories = await _categoryRepository.GetByIdsAsync(dto.CategoryIds);
        }

        await _bookRepository.AddAsync(book);

        return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, ToDto(book));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Librarian}")]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookDto dto)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null) return NotFound();

        var isbn = dto.ISBN.Trim();
        if (await _bookRepository.IsbnExistsAsync(isbn, id))
            return Conflict("A book with this ISBN already exists.");

        book.Title = dto.Title.Trim();
        book.ISBN = isbn;
        book.Author = dto.Author.Trim();
        book.Available = dto.Available;
        book.Categories = await _categoryRepository.GetByIdsAsync(dto.CategoryIds);

        await _bookRepository.UpdateAsync(book);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Librarian}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null) return NotFound();

        book.IsDeleted = true;
        await _bookRepository.UpdateAsync(book);
        return NoContent();
    }

    private static BookDto ToDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        ISBN = book.ISBN,
        Author = book.Author,
        Available = book.Available,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies,
        Categories = book.Categories.Select(c => c.Name).ToList()
    };
}
