using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeeMart.Tests;

public class TransactionServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BeeMartDbContext _context;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BeeMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BeeMartDbContext(options);
        _context.Database.EnsureCreated();

        _transactionService = new TransactionService(_context, NullLogger<TransactionService>.Instance);

        // Seed initial data
        _context.Users.Add(new MsUser
        {
            IdUser = "76985",
            Username = "steven",
            FullName = "Steven Anderson",
            PasswordHash = "hash",
            Role = "Cashier",
            Stsrc = true
        });

        _context.PaymentMethods.Add(new LtPaymentMethod
        {
            IdPaymentMethod = 1,
            MethodName = "Cash",
            Stsrc = true
        });

        _context.Products.Add(new MsProduct
        {
            IdProduct = "prod-1",
            Name = "Apel Fuji",
            Weight = 1.0m,
            Stock = 10,
            Price = 40000,
            Discount = 10, // 10% discount -> 36000
            Stsrc = true
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateTransaction_QuantityExceedsStock_ReturnsExactErrorMessage()
    {
        var request = new CreateTransactionRequest
        {
            IdUser = "76985",
            IdPaymentMethod = 1,
            Items = new List<CreateTransactionItemRequest>
            {
                new() { IdProduct = "prod-1", Qty = 15 } // Stock is 10, requested 15
            }
        };

        var result = await _transactionService.CreateTransactionAsync(request);

        Assert.False(result.Success);
        // Specification requirement: "Jumlah barang yang dimasukkan melebihi sisa stok."
        Assert.Equal("Jumlah barang yang dimasukkan melebihi sisa stok.", result.Message);
    }

    [Fact]
    public async Task CreateTransaction_Valid_DeductsStockAndCalculatesCorrectSubtotal()
    {
        var request = new CreateTransactionRequest
        {
            IdUser = "76985",
            IdPaymentMethod = 1,
            Items = new List<CreateTransactionItemRequest>
            {
                new() { IdProduct = "prod-1", Qty = 2 }
            }
        };

        var result = await _transactionService.CreateTransactionAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        // Price: 40,000 with 10% discount is 36,000 each. 2 items = 72,000.
        Assert.Equal(72000m, result.Data.TotalPrice);

        // Verify stock deducted: 10 - 2 = 8
        var updatedProduct = await _context.Products.FindAsync("prod-1");
        Assert.NotNull(updatedProduct);
        Assert.Equal(8, updatedProduct.Stock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
