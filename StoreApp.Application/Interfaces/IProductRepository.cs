using Store.Domain.Entities;

namespace StoreApp.Application.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<bool> AnyByCategoryIdAsync(int categoryId);
    Task AddAsync(Product product);
    Task SaveChangesAsync();
}