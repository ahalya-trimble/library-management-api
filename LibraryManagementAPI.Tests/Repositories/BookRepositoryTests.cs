using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementAPI.Tests.Repositories;

public class BookRepositoryTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenBookIsSoftDeleted()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        var book = new Book { Title = "Title", Author = "Author", ISBN = "1234567890", IsDeleted = true };
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(book.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task IsbnExistsAsync_ReturnsTrue_WhenIsbnExists()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        await repository.AddAsync(new Book { Title = "Title", Author = "Author", ISBN = "1234567890" });

        var exists = await repository.IsbnExistsAsync("1234567890");

        Assert.True(exists);
    }

    [Fact]
    public async Task IsbnExistsAsync_ExcludesGivenId()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        var book = new Book { Title = "Title", Author = "Author", ISBN = "1234567890" };
        await repository.AddAsync(book);

        var exists = await repository.IsbnExistsAsync("1234567890", book.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task GetAvailableBooksAsync_ReturnsOnlyAvailableAndNonDeletedBooks()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        await repository.AddAsync(new Book { Title = "Available", Author = "A", ISBN = "1111111111", Available = true });
        await repository.AddAsync(new Book { Title = "Unavailable", Author = "B", ISBN = "2222222222", Available = false });
        await repository.AddAsync(new Book { Title = "Deleted", Author = "C", ISBN = "3333333333", Available = true, IsDeleted = true });

        var result = await repository.GetAvailableBooksAsync();

        Assert.Single(result);
        Assert.Equal("Available", result.First().Title);
    }

    [Fact]
    public async Task GetBooksAsync_FiltersByAuthor()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        await repository.AddAsync(new Book { Title = "Book1", Author = "George Orwell", ISBN = "1111111111" });
        await repository.AddAsync(new Book { Title = "Book2", Author = "Aldous Huxley", ISBN = "2222222222" });

        var result = await repository.GetBooksAsync(new BookQueryParameters { Author = "Orwell" });

        Assert.Single(result.Items);
        Assert.Equal("George Orwell", result.Items.First().Author);
    }

    [Fact]
    public async Task GetBooksAsync_SortsByAuthorDescending()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        await repository.AddAsync(new Book { Title = "Book1", Author = "Aldous Huxley", ISBN = "1111111111" });
        await repository.AddAsync(new Book { Title = "Book2", Author = "George Orwell", ISBN = "2222222222" });

        var result = await repository.GetBooksAsync(new BookQueryParameters { SortBy = "author", SortOrder = "desc" });

        Assert.Equal("George Orwell", result.Items.First().Author);
        Assert.Equal("Aldous Huxley", result.Items.Last().Author);
    }

    [Fact]
    public async Task GetBooksAsync_DefaultsToTitleAscending_ForUnknownSortBy()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        await repository.AddAsync(new Book { Title = "Zebra", Author = "A", ISBN = "1111111111" });
        await repository.AddAsync(new Book { Title = "Apple", Author = "B", ISBN = "2222222222" });

        var result = await repository.GetBooksAsync(new BookQueryParameters { SortBy = "unknown" });

        Assert.Equal("Apple", result.Items.First().Title);
        Assert.Equal("Zebra", result.Items.Last().Title);
    }

    [Fact]
    public async Task GetBooksAsync_AppliesPaging()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);
        for (var i = 1; i <= 5; i++)
        {
            await repository.AddAsync(new Book { Title = $"Book{i}", Author = "Author", ISBN = $"{i}".PadLeft(10, '0') });
        }

        var result = await repository.GetBooksAsync(new BookQueryParameters { PageNumber = 2, PageSize = 2 });

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
    }
}
