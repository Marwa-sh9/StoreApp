using Store.Domain.Entities;
using StoreApp.Application.DTOs;
using StoreApp.Application.Exceptions;
using StoreApp.Application.Interfaces;

namespace StoreApp.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(ToDto);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        return ToDto(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _categoryRepository.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        // Block deletion (409) if the category still has active products.
        if (await _productRepository.AnyByCategoryIdAsync(id))
        {
            throw new ConflictException("Cannot delete this category because it contains active products.");
        }

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;

        await _categoryRepository.SaveChangesAsync();
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description
    };
}