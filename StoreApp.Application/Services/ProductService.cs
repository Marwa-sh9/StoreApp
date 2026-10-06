using Store.Domain.Entities;
using StoreApp.Application.DTOs;
using StoreApp.Application.Exceptions;
using StoreApp.Application.Interfaces;

namespace StoreApp.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(ToDto);
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product not found.");

        return ToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Duplicate SKU -> 409 (includes soft-deleted products).
        if (await _productRepository.GetBySkuAsync(dto.SKU) != null)
        {
            throw new ConflictException("A product with this SKU already exists.");
        }

        var category = await GetActiveCategoryOrThrowAsync(dto.CategoryId);

        var product = new Product
        {
            Name = dto.Name,
            SKU = dto.SKU,
            Price = dto.Price,
            QuantityInStock = dto.QuantityInStock,
            CategoryId = dto.CategoryId,
            Category = category
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return ToDto(product);
    }

    public async Task UpdateProductAsync(int id, CreateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product not found.");

        var productWithSameSku = await _productRepository.GetBySkuAsync(dto.SKU);
        if (productWithSameSku != null && productWithSameSku.Id != id)
        {
            throw new ConflictException("A product with this SKU already exists.");
        }

        var category = await GetActiveCategoryOrThrowAsync(dto.CategoryId);

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.Price = dto.Price;
        product.QuantityInStock = dto.QuantityInStock;
        product.Category = category;

        await _productRepository.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product not found.");

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;

        await _productRepository.SaveChangesAsync();
    }

    private async Task<Category> GetActiveCategoryOrThrowAsync(int categoryId)
    {
        // The repository already excludes soft-deleted categories.
        return await _categoryRepository.GetByIdAsync(categoryId)
            ?? throw new NotFoundException("The specified category does not exist or is deleted.");
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        SKU = p.SKU,
        Price = p.Price,
        QuantityInStock = p.QuantityInStock,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name
    };
}