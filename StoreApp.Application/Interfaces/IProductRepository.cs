using Store.Domain.Entities;

namespace StoreApp.Application.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);

    Task<Product?> GetBySkuAsync(string sku);
    Task AddAsync(Product product);
    void Update(Product product);
    Task SaveChangesAsync();
}