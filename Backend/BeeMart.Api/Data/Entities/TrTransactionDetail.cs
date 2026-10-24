using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Data.Entities;

[Table("TrTransactionDetail")]
public class TrTransactionDetail
{
    [Key]
    [StringLength(36)]
    public string IdTransactionDetail { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(36)]
    public string IdTransaction { get; set; } = string.Empty;

    [Required]
    [StringLength(36)]
    public string IdProduct { get; set; } = string.Empty;

    public int Qty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; } = 0.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [StringLength(36)]
    public string? Userin { get; set; }

    public DateTime? Dateup { get; set; }

    [StringLength(36)]
    public string? Userup { get; set; }

    // Navigation properties
    [ForeignKey("IdTransaction")]
    public virtual TrTransaction? Transaction { get; set; }

    [ForeignKey("IdProduct")]
    public virtual MsProduct? Product { get; set; }
}
