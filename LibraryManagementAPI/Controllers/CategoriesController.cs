using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(ToDto(category));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await _categoryRepository.NameExistsAsync(name))
            return Conflict("A category with this name already exists.");

        var category = new Category { Name = name };
        await _categoryRepository.AddAsync(category);

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, ToDto(category));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CreateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null) return NotFound();

        var name = dto.Name.Trim();
        if (await _categoryRepository.NameExistsAsync(name, id))
            return Conflict("A category with this name already exists.");

        category.Name = name;
        await _categoryRepository.UpdateAsync(category);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null) return NotFound();

        await _categoryRepository.DeleteAsync(category);
        return NoContent();
    }

    private static CategoryDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name
    };
}
