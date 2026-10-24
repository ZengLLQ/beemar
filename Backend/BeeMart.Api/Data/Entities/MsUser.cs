using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Data.Entities;

[Table("MsUser")]
public class MsUser
{
    [Key]
    [StringLength(5)]
    public string IdUser { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "Cashier"; // "Admin" or "Cashier"

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
