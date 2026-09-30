using LibraryManagementAPI.Controllers;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryManagementAPI.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock = new();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetCategories_ReturnsOkWithMappedDtos()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction" },
            new() { Id = 2, Name = "Science" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _controller.GetCategories();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        var result = await _controller.GetCategoryById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsOk_WhenFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Fiction" });

        var result = await _controller.GetCategoryById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal("Fiction", dto.Name);
    }

    [Fact]
    public async Task CreateCategory_ReturnsConflict_WhenNameExists()
    {
        _repositoryMock.Setup(r => r.NameExistsAsync("Fiction", null)).ReturnsAsync(true);

        var result = await _controller.CreateCategory(new CreateCategoryDto { Name = "Fiction" });

        Assert.IsType<ConflictObjectResult>(result.Result);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreatedAtAction_WhenNameIsUnique()
    {
        _repositoryMock.Setup(r => r.NameExistsAsync("Fiction", null)).ReturnsAsync(false);

        var result = await _controller.CreateCategory(new CreateCategoryDto { Name = "Fiction" });

        Assert.IsType<CreatedAtActionResult>(result.Result);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "Fiction")), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        var result = await _controller.DeleteCategory(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNoContent_WhenDeleted()
    {
        var category = new Category { Id = 1, Name = "Fiction" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _controller.DeleteCategory(1);

        Assert.IsType<NoContentResult>(result);
        _repositoryMock.Verify(r => r.DeleteAsync(category), Times.Once);
    }
}
