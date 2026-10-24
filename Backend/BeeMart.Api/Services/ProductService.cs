using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public interface IProductService
{
    Task<List<ProductDto>> SearchProductsAsync(string? query);
    Task<ProductDto?> GetProductByIdAsync(string id);
}

public class ProductService : IProductService
{
    private readonly BeeMartDbContext _context;

    public ProductService(BeeMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> SearchProductsAsync(string? query)
    {
        var dbQuery = _context.Products.AsNoTracking().Where(p => p.Stsrc);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var cleanQuery = query.Trim().ToLower();
            dbQuery = dbQuery.Where(p => p.Name.ToLower().Contains(cleanQuery));
        }

        var results = await dbQuery
            .OrderBy(p => p.Name)
            .Take(10) // Max 10 data
            .Select(p => new ProductDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Weight = p.Weight,
                Stock = p.Stock,
                Price = p.Price,
                Discount = p.Discount
            })
            .ToListAsync();

        return results;
    }

    public async Task<ProductDto?> GetProductByIdAsync(string id)
    {
        var product = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProduct == id && p.Stsrc);

        if (product == null) return null;

        return new ProductDto
        {
            IdProduct = product.IdProduct,
            Name = product.Name,
            Weight = product.Weight,
            Stock = product.Stock,
            Price = product.Price,
            Discount = product.Discount
        };
    }
}
