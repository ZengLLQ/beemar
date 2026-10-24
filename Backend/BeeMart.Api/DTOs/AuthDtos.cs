using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    public string Role { get; set; } = string.Empty; // "Admin" or "Cashier"
}

public class LoginResponse
{
    public string IdUser { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name wajib diisi.")]
    [StringLength(300, ErrorMessage = "Full Name maksimal 300 karakter.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Password minimal 4 karakter.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Re-Password wajib diisi.")]
    [Compare("Password", ErrorMessage = "Password and Re-Password do not match.")]
    public string RePassword { get; set; } = string.Empty;

    public string Role { get; set; } = "Cashier";
}
