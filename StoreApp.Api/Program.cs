using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StoreApp.Api.Filters;
using StoreApp.Api.Middleware;
using StoreApp.Application.Interfaces;
using StoreApp.Application.Services;
using StoreApp.Application.Validators;
using StoreApp.Infrastructure;
using StoreApp.Infrastructure.Data;
using StoreApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Must be first so it catches exceptions from everything after it.
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database initialization failed. Check the 'DefaultConnection' string in appsettings.json and make sure SQL Server is running.");
        throw; // fail fast instead of running with a broken database
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();