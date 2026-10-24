using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeeMart.Tests;

public class InvoiceNumberTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BeeMartDbContext _context;
    private readonly TransactionService _transactionService;

    public InvoiceNumberTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BeeMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BeeMartDbContext(options);
        _context.Database.EnsureCreated();

        _context.Users.Add(new MsUser
        {
            IdUser = "76985",
            Username = "steven",
            FullName = "Steven",
            PasswordHash = "h",
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

        _transactionService = new TransactionService(_context, NullLogger<TransactionService>.Instance);
    }

    [Fact]
    public async Task GenerateNextInvoiceNo_FirstTransactionOfDay_ReturnsStartingWith001()
    {
        // Arrange
        var userId = "76985";
        var date = new DateTime(2026, 4, 9, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var invoiceNo = await _transactionService.GenerateNextInvoiceNoAsync(userId, date);

        // Assert: 76985 + 090426 + 001 = 76985090426001
        Assert.Equal("76985090426001", invoiceNo);
    }

    [Fact]
    public async Task GenerateNextInvoiceNo_WithExistingInvoices_IncrementsRunningNumber()
    {
        // Arrange
        var userId = "76985";
        var date = new DateTime(2026, 4, 9, 10, 0, 0, DateTimeKind.Utc);

        _context.Transactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "76985090426001",
            IdUser = userId,
            IdPaymentMethod = 1,
            TotalPrice = 10000,
            TransactionDate = date
        });
        _context.Transactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "76985090426002",
            IdUser = userId,
            IdPaymentMethod = 1,
            TotalPrice = 20000,
            TransactionDate = date
        });
        await _context.SaveChangesAsync();

        // Act
        var nextInvoice = await _transactionService.GenerateNextInvoiceNoAsync(userId, date);

        // Assert
        Assert.Equal("76985090426003", nextInvoice);
    }

    [Fact]
    public async Task GenerateNextInvoiceNo_ShortUserId_PadsLeftWithZeros()
    {
        // Arrange: User id with less than 5 characters
        var userId = "123";
        var date = new DateTime(2026, 9, 4, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var invoiceNo = await _transactionService.GenerateNextInvoiceNoAsync(userId, date);

        // Assert: "00123" + "040926" + "001" = 00123040926001
        Assert.Equal("00123040926001", invoiceNo);
    }

    [Fact]
    public async Task GenerateNextInvoiceNo_MaxRunningNumber999_RollsOverSafely()
    {
        var userId = "76985";
        var date = new DateTime(2026, 9, 4, 10, 0, 0, DateTimeKind.Utc);

        _context.Transactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "76985040926999",
            IdUser = userId,
            IdPaymentMethod = 1,
            TotalPrice = 50000,
            TransactionDate = date
        });
        await _context.SaveChangesAsync();

        var invoiceNo = await _transactionService.GenerateNextInvoiceNoAsync(userId, date);

        // Should roll over to 001 and maintain exact 14 digits format
        Assert.Equal("76985040926001", invoiceNo);
        Assert.Equal(14, invoiceNo.Length);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
