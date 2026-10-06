using StoreApp.Application.DTOs;
using StoreApp.Application.Interfaces;
using StoreApp.Application.Exceptions;
using Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _categoryRepository.AddAsync(category);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new NotFoundException("Category not found");

        category.Name = dto.Name;
        category.Description = dto.Description;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new NotFoundException("Category not found");

        // منع الحذف وإرجاع Conflict (409) في حال وجود منتجات فعّالة مرتبطة بهذا التصنيف
        var products = await _productRepository.GetAllAsync();
        var hasActiveProducts = products.Any(p => p.CategoryId == id && !p.IsDeleted);
        if (hasActiveProducts)
        {
            throw new ConflictException("Cannot delete this category because it contains active products.");
        }

        // تفعيل الحذف الناعم وتخزين تاريخ ووقت الحذف
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();
    }
}