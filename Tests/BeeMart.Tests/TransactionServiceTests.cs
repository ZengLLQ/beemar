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

    [Fact]
    public async Task CreateTransaction_MultipleEntriesSameProductExceedingStock_ReturnsExactErrorMessage()
    {
        // Product stock is 10. Two entries of 6 = 12 total requested.
        var request = new CreateTransactionRequest
        {
            IdUser = "76985",
            IdPaymentMethod = 1,
            Items = new List<CreateTransactionItemRequest>
            {
                new() { IdProduct = "prod-1", Qty = 6 },
                new() { IdProduct = "prod-1", Qty = 6 }
            }
        };

        var result = await _transactionService.CreateTransactionAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Jumlah barang yang dimasukkan melebihi sisa stok.", result.Message);

        // Stock should remain unmodified (10)
        var product = await _context.Products.FindAsync("prod-1");
        Assert.NotNull(product);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public async Task CreateTransaction_DuplicateInvoiceNumber_ThrowsDbUpdateException()
    {
        var date = new DateTime(2026, 4, 9, 10, 0, 0, DateTimeKind.Utc);
        _context.Transactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "76985090426001",
            IdUser = "76985",
            IdPaymentMethod = 1,
            TotalPrice = 10000,
            TransactionDate = date
        });
        await _context.SaveChangesAsync();

        _context.Transactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "76985090426001", // Duplicate InvoiceNo
            IdUser = "76985",
            IdPaymentMethod = 1,
            TotalPrice = 20000,
            TransactionDate = date
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task CreateTransaction_ConcurrentParallelThreads_GeneratesDistinctSequentialInvoices()
    {
        // Add a product with plenty of stock
        _context.Products.Add(new MsProduct
        {
            IdProduct = "prod-stock",
            Name = "Stok Melimpah",
            Weight = 1.0m,
            Stock = 100,
            Price = 10000,
            Discount = 0,
            Stsrc = true
        });
        await _context.SaveChangesAsync();

        // 5 concurrent requests from the same cashier user
        const int concurrentCount = 5;
        var tasks = Enumerable.Range(1, concurrentCount).Select(async _ =>
        {
            // Each parallel task gets its own DbContext sharing the in-memory SQLite connection
            var options = new DbContextOptionsBuilder<BeeMartDbContext>()
                .UseSqlite(_connection)
                .Options;
            using var taskContext = new BeeMartDbContext(options);
            var service = new TransactionService(taskContext, NullLogger<TransactionService>.Instance);

            var req = new CreateTransactionRequest
            {
                IdUser = "76985",
                IdPaymentMethod = 1,
                Items = new List<CreateTransactionItemRequest>
                {
                    new() { IdProduct = "prod-stock", Qty = 1 }
                }
            };

            return await service.CreateTransactionAsync(req);
        });

        var results = await Task.WhenAll(tasks);

        // Verify all 5 succeeded
        Assert.All(results, r => Assert.True(r.Success));

        // Verify all 5 invoice numbers are unique
        var invoiceNumbers = results.Select(r => r.Data!.InvoiceNo).ToList();
        var uniqueCount = invoiceNumbers.Distinct().Count();
        Assert.Equal(concurrentCount, uniqueCount);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
