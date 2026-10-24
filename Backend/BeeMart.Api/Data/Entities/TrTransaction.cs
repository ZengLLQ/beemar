using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Data.Entities;

[Table("TrTransaction")]
public class TrTransaction
{
    [Key]
    [StringLength(36)]
    public string IdTransaction { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(20)]
    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(5)]
    public string IdUser { get; set; } = string.Empty;

    public int IdPaymentMethod { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [StringLength(36)]
    public string? Userin { get; set; }

    public DateTime? Dateup { get; set; }

    [StringLength(36)]
    public string? Userup { get; set; }

    // Navigation properties
    [ForeignKey("IdUser")]
    public virtual MsUser? User { get; set; }

    [ForeignKey("IdPaymentMethod")]
    public virtual LtPaymentMethod? PaymentMethod { get; set; }

    public virtual ICollection<TrTransactionDetail> TransactionDetails { get; set; } = new List<TrTransactionDetail>();
}
