using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        var products = await _productService.SearchProductsAsync(q);
        return Ok(ApiResponse<List<ProductDto>>.Ok(products));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse<ProductDto>.Fail("Produk tidak ditemukan."));
        }
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }
}
