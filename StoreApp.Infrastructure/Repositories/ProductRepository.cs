using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
using StoreApp.Application.Exceptions;
using StoreApp.Application.Interfaces;

namespace StoreApp.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // The global query filter already excludes soft-deleted products.
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        // Tracked on purpose: the service updates this entity and saves.
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        // IgnoreQueryFilters so a soft-deleted product's SKU cannot be reused
        // (the unique index still covers deleted rows).
        return await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<bool> AnyByCategoryIdAsync(int categoryId)
    {
        // Query filter excludes soft-deleted products automatically.
        return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Covers the race condition where two requests use the same SKU at once.
            throw new ConflictException("A product with this SKU already exists.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is Microsoft.Data.SqlClient.SqlException sql &&
        (sql.Number == 2601 || sql.Number == 2627);
}