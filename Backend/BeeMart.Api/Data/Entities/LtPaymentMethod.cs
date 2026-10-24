using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Data.Entities;

[Table("LtPaymentMethod")]
public class LtPaymentMethod
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdPaymentMethod { get; set; }

    [Required]
    [StringLength(50)]
    public string MethodName { get; set; } = string.Empty; // Cash, Debit, Credit

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [StringLength(36)]
    public string? Userin { get; set; }

    public DateTime? Dateup { get; set; }

    [StringLength(36)]
    public string? Userup { get; set; }

    // Navigation properties
    public virtual ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
}
