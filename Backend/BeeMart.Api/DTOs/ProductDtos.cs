namespace BeeMart.Api.DTOs;

public class ProductDto
{
    public string IdProduct { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }

    public string WeightFormatted => $"{Weight:0.##} Kg";
    public decimal FinalUnitPrice => Price - (Price * (Discount / 100m));
}
