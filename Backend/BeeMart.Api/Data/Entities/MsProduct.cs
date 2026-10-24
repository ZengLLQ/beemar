using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Data.Entities;

[Table("MsProduct")]
public class MsProduct
{
    [Key]
    [StringLength(36)]
    public string IdProduct { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Weight { get; set; }

    public int Stock { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; } = 0.00m;

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [StringLength(36)]
    public string? Userin { get; set; }

    public DateTime? Dateup { get; set; }

    [StringLength(36)]
    public string? Userup { get; set; }

    // Navigation properties
    public virtual ICollection<TrTransactionDetail> TransactionDetails { get; set; } = new List<TrTransactionDetail>();
}
