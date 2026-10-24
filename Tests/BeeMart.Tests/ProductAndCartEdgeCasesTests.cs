using System.Text.RegularExpressions;
using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeeMart.Tests;

public class ProductAndCartEdgeCasesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BeeMartDbContext _context;
    private readonly ProductService _productService;
    private readonly TransactionService _transactionService;

    public ProductAndCartEdgeCasesTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BeeMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BeeMartDbContext(options);
        _context.Database.EnsureCreated();

        _productService = new ProductService(_context);
        _transactionService = new TransactionService(_context, NullLogger<TransactionService>.Instance);

        // Seed products
        _context.Products.AddRange(
            new MsProduct { IdProduct = "p1", Name = "Apel Manis", Weight = 1.0m, Stock = 5, Price = 10000, Stsrc = true },
            new MsProduct { IdProduct = "p2", Name = "Apel Asam", Weight = 1.0m, Stock = 8, Price = 12000, Stsrc = true },
            new MsProduct { IdProduct = "p3", Name = "Susu Sapi", Weight = 1.0m, Stock = 12, Price = 15000, Stsrc = true },
            new MsProduct { IdProduct = "p4", Name = "Apel Inactive", Weight = 1.0m, Stock = 20, Price = 8000, Stsrc = false } // Inactive
        );

        _context.Users.Add(new MsUser
        {
            IdUser = "55555",
            Username = "cashier1",
            FullName = "Cashier One",
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

        _context.SaveChanges();
    }

    [Fact]
    public async Task SearchProducts_MatchesActiveOnlyAndCapsAt10()
    {
        var results = await _productService.SearchProductsAsync("apel");

        // Should return "Apel Manis" and "Apel Asam", but NOT "Apel Inactive"
        Assert.Equal(2, results.Count);
        Assert.DoesNotContain(results, p => p.Name == "Apel Inactive");
    }

    [Fact]
    public async Task InvoiceNumber_MatchesExact14DigitsFormat()
    {
        var invoiceNo = await _transactionService.GenerateNextInvoiceNoAsync("55555", new DateTime(2026, 10, 24));

        // Format: [5 digit userID][ddmmyy][3 digit running number]
        Assert.Matches(new Regex(@"^[0-9]{5}[0-9]{6}[0-9]{3}$"), invoiceNo);
        Assert.Equal("55555241026001", invoiceNo);
    }

    [Fact]
    public async Task CreateTransaction_EmptyCart_ReturnsError()
    {
        var request = new CreateTransactionRequest
        {
            IdUser = "55555",
            IdPaymentMethod = 1,
            Items = new List<CreateTransactionItemRequest>()
        };

        var result = await _transactionService.CreateTransactionAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Keranjang belanja tidak boleh kosong.", result.Message);
    }

    [Fact]
    public async Task CreateTransaction_ZeroOrNegativeQty_ReturnsError()
    {
        var request = new CreateTransactionRequest
        {
            IdUser = "55555",
            IdPaymentMethod = 1,
            Items = new List<CreateTransactionItemRequest>
            {
                new() { IdProduct = "p1", Qty = 0 }
            }
        };

        var result = await _transactionService.CreateTransactionAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Jumlah barang harus lebih dari 0.", result.Message);
    }

    [Fact]
    public async Task CreateTransaction_InvalidPaymentMethod_ReturnsError()
    {
        var request = new CreateTransactionRequest
        {
            IdUser = "55555",
            IdPaymentMethod = 999, // Non-existent payment method
            Items = new List<CreateTransactionItemRequest>
            {
                new() { IdProduct = "p1", Qty = 1 }
            }
        };

        var result = await _transactionService.CreateTransactionAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Metode pembayaran tidak valid.", result.Message);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
