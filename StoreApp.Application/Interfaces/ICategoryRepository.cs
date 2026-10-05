using Store.Domain.Entities;

namespace StoreApp.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task AddAsync(Category category);
    void Update(Category category);     
    Task SaveChangesAsync();            
}