using LibraryManagementAPI.Data;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementAPI.Tests.Repositories;

public class CategoryRepositoryTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AddsCategory_ThatCanBeRetrieved()
    {
        using var context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "Fiction" };

        await repository.AddAsync(category);
        var result = await repository.GetByIdAsync(category.Id);

        Assert.NotNull(result);
        Assert.Equal("Fiction", result!.Name);
    }

    [Fact]
    public async Task NameExistsAsync_ReturnsTrue_WhenNameExists_CaseInsensitive()
    {
        using var context = CreateContext();
        var repository = new CategoryRepository(context);
        await repository.AddAsync(new Category { Name = "Science" });

        var exists = await repository.NameExistsAsync("science");

        Assert.True(exists);
    }

    [Fact]
    public async Task NameExistsAsync_ReturnsFalse_WhenNameDoesNotExist()
    {
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var exists = await repository.NameExistsAsync("NonExistent");

        Assert.False(exists);
    }

    [Fact]
    public async Task NameExistsAsync_ExcludesGivenId()
    {
        using var context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "History" };
        await repository.AddAsync(category);

        var exists = await repository.NameExistsAsync("History", category.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        using var context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "ToDelete" };
        await repository.AddAsync(category);

        await repository.DeleteAsync(category);
        var result = await repository.GetByIdAsync(category.Id);

        Assert.Null(result);
    }
}
