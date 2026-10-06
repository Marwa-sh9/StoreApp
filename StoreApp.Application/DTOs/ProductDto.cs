namespace StoreApp.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public int CategoryId { get; set; }
}