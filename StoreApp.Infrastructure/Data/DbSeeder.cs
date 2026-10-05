using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;

namespace StoreApp.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // التأكد من تطبيق أي migrations معلقة
        await context.Database.MigrateAsync();

        // التحقق مما إذا كانت الأقسام موجودة مسبقاً لمنع تكرار البيانات
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Clear Aligners", Description = "Custom-made invisible aligners for teeth straightening" },
                new Category { Name = "Dental Care Kits", Description = "Essential kits for maintaining aligner hygiene and oral health" },
                new Category { Name = "Retainers & Accessories", Description = "Post-treatment retainers, cases, and cleaning tools" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // التحقق مما إذا كانت المنتجات موجودة مسبقاً
        if (!await context.Products.AnyAsync())
        {
            // جلب الأقسام المضافة للحصول على الـ IDs الخاصة بها
            var alignersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Clear Aligners");
            var careCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Dental Care Kits");
            var accessoriesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Retainers & Accessories");

            var products = new List<Product>();

            if (alignersCategory != null)
            {
                products.Add(new Product { Name = "BClear Full Treatment Aligner", SKU = "ALIGN-001", Price = 1500.00m, CategoryId = alignersCategory.Id });
                products.Add(new Product { Name = "BClear Express (Minor Correction)", SKU = "ALIGN-002", Price = 850.00m, CategoryId = alignersCategory.Id });
            }

            if (careCategory != null)
            {
                products.Add(new Product { Name = "Complete Aligner Cleaning Kit", SKU = "CARE-001", Price = 35.00m, CategoryId = careCategory.Id });
                products.Add(new Product { Name = "Whitening Foam for Aligners", SKU = "CARE-002", Price = 25.00m, CategoryId = careCategory.Id });
            }

            if (accessoriesCategory != null)
            {
                products.Add(new Product { Name = "Post-Treatment Retainer Set", SKU = "ACC-001", Price = 200.00m, CategoryId = accessoriesCategory.Id });
                products.Add(new Product { Name = "Antibacterial Aligner Storage Case", SKU = "ACC-002", Price = 15.00m, CategoryId = accessoriesCategory.Id });
            }

            if (products.Any())
            {
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}