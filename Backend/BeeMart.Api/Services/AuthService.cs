using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request);
}

public class AuthService : IAuthService
{
    private readonly BeeMartDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(BeeMartDbContext context, IPasswordHasher passwordHasher, ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("Invalid login");
        }

        var normalizedUsername = request.Username.Trim().ToLower();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername && u.Stsrc);

        if (user == null)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid login");
        }

        // Validate Role (case-insensitive)
        if (!string.Equals(user.Role, request.Role, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<LoginResponse>.Fail("Invalid login");
        }

        // Validate Password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.Fail("Invalid login");
        }

        var response = new LoginResponse
        {
            IdUser = user.IdUser,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Token = Guid.NewGuid().ToString("N") // Session token
        };

        return ApiResponse<LoginResponse>.Ok(response, "Login berhasil.");
    }

    public async Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.RePassword))
        {
            return ApiResponse<LoginResponse>.Fail("Semua field wajib diisi.");
        }

        if (request.Password != request.RePassword)
        {
            return ApiResponse<LoginResponse>.Fail("Password and Re-Password do not match.");
        }

        var normalizedUsername = request.Username.Trim().ToLower();

        // Check if username exists (Specification: "username has already exists")
        var existingUser = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == normalizedUsername);

        if (existingUser)
        {
            return ApiResponse<LoginResponse>.Fail("username has already exists");
        }

        // Generate unique 5-digit IdUser
        var newIdUser = await GenerateUniqueUserIdAsync();

        var role = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Cashier";

        var newUser = new MsUser
        {
            IdUser = newIdUser,
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = role,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = newIdUser
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        var response = new LoginResponse
        {
            IdUser = newUser.IdUser,
            Username = newUser.Username,
            FullName = newUser.FullName,
            Role = newUser.Role,
            Token = Guid.NewGuid().ToString("N")
        };

        return ApiResponse<LoginResponse>.Ok(response, "Registrasi berhasil.");
    }

    private async Task<string> GenerateUniqueUserIdAsync()
    {
        var random = new Random();
        for (int i = 0; i < 1000; i++)
        {
            var candidate = random.Next(10000, 99999).ToString();
            var exists = await _context.Users.AnyAsync(u => u.IdUser == candidate);
            if (!exists)
            {
                return candidate;
            }
        }

        // Fallback sequential
        for (int candidate = 10000; candidate <= 99999; candidate++)
        {
            var strCandidate = candidate.ToString();
            var exists = await _context.Users.AnyAsync(u => u.IdUser == strCandidate);
            if (!exists)
            {
                return strCandidate;
            }
        }

        throw new InvalidOperationException("Tidak dapat membuat User ID 5 digit yang unik.");
    }
}
