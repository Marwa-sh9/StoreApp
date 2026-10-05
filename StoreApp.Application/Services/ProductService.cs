using StoreApp.Application.DTOs;
using StoreApp.Application.Interfaces;
using Store.Domain.Entities;

namespace StoreApp.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name
        });
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var p = await _productRepository.GetByIdAsync(id);
        if (p == null) return null;

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        
        var existingProduct = await _productRepository.GetBySkuAsync(dto.SKU);
        if (existingProduct != null)
        {
            throw new Exception("A product with this SKU already exists.");
        }

        var product = new Product
        {
            Name = dto.Name,
            SKU = dto.SKU,
            Price = dto.Price,
            CategoryId = dto.CategoryId
        };

        await _productRepository.AddAsync(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Price = product.Price,
            CategoryId = product.CategoryId
        };
    }

    public async Task UpdateProductAsync(int id, CreateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) throw new Exception("Product not found");

        var existingProductWithSku = await _productRepository.GetBySkuAsync(dto.SKU);
        if (existingProductWithSku != null && existingProductWithSku.Id != id)
        {
            throw new Exception("A product with this SKU already exists.");
        }

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) throw new Exception("Product not found");

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();
    }
}