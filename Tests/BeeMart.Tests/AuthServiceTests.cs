using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeeMart.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BeeMartDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BeeMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BeeMartDbContext(options);
        _context.Database.EnsureCreated();

        _passwordHasher = new PasswordHasher();
        _authService = new AuthService(_context, _passwordHasher, NullLogger<AuthService>.Instance);

        // Seed sample user
        _context.Users.Add(new MsUser
        {
            IdUser = "76985",
            Username = "steven",
            FullName = "Steven Anderson",
            PasswordHash = _passwordHasher.HashPassword("secret123"),
            Role = "Cashier",
            Stsrc = true
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Login_ValidCashierCredentials_ReturnsSuccess()
    {
        var req = new LoginRequest
        {
            Username = "steven",
            Password = "secret123",
            Role = "Cashier"
        };

        var res = await _authService.LoginAsync(req);

        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal("76985", res.Data.IdUser);
        Assert.Equal("Cashier", res.Data.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsInvalidLogin()
    {
        var req = new LoginRequest
        {
            Username = "steven",
            Password = "wrongPassword",
            Role = "Cashier"
        };

        var res = await _authService.LoginAsync(req);

        Assert.False(res.Success);
        Assert.Equal("Invalid login", res.Message);
    }

    [Fact]
    public async Task Login_WrongRole_ReturnsInvalidLogin()
    {
        // Steven is Cashier, but logs in requesting Admin
        var req = new LoginRequest
        {
            Username = "steven",
            Password = "secret123",
            Role = "Admin"
        };

        var res = await _authService.LoginAsync(req);

        Assert.False(res.Success);
        Assert.Equal("Invalid login", res.Message);
    }

    [Theory]
    [InlineData("", "password", "Cashier")]
    [InlineData("steven", "", "Cashier")]
    [InlineData("   ", "   ", "Cashier")]
    public async Task Login_EmptyCredentials_ReturnsInvalidLogin(string username, string password, string role)
    {
        var req = new LoginRequest
        {
            Username = username,
            Password = password,
            Role = role
        };

        var res = await _authService.LoginAsync(req);

        Assert.False(res.Success);
        Assert.Equal("Invalid login", res.Message);
    }

    [Fact]
    public async Task Register_ExistingUsername_ReturnsUsernameHasAlreadyExists()
    {
        var req = new RegisterRequest
        {
            Username = "steven", // Already exists
            FullName = "Steven Second",
            Password = "mypassword",
            RePassword = "mypassword"
        };

        var res = await _authService.RegisterAsync(req);

        Assert.False(res.Success);
        Assert.Equal("username has already exists", res.Message);
    }

    [Fact]
    public async Task Register_PasswordMismatch_ReturnsErrorMessage()
    {
        var req = new RegisterRequest
        {
            Username = "newuser",
            FullName = "New User",
            Password = "passwordA",
            RePassword = "passwordB"
        };

        var res = await _authService.RegisterAsync(req);

        Assert.False(res.Success);
        Assert.Equal("Password and Re-Password do not match.", res.Message);
    }

    [Fact]
    public async Task Register_Valid_Generates5DigitUserIdAndHashesPassword()
    {
        var req = new RegisterRequest
        {
            Username = "johndoe",
            FullName = "John Doe",
            Password = "password123",
            RePassword = "password123",
            Role = "Cashier"
        };

        var res = await _authService.RegisterAsync(req);

        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal(5, res.Data.IdUser.Length);

        var savedUser = await _context.Users.FindAsync(res.Data.IdUser);
        Assert.NotNull(savedUser);
        Assert.NotEqual("password123", savedUser.PasswordHash);
        Assert.True(_passwordHasher.VerifyPassword("password123", savedUser.PasswordHash));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
