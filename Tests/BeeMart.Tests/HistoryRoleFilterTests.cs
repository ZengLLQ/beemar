using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeeMart.Tests;

public class HistoryRoleFilterTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BeeMartDbContext _context;
    private readonly TransactionService _transactionService;

    public HistoryRoleFilterTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BeeMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BeeMartDbContext(options);
        _context.Database.EnsureCreated();

        _transactionService = new TransactionService(_context, NullLogger<TransactionService>.Instance);

        // Seed users: 2 Cashiers and 1 Admin
        _context.Users.AddRange(
            new MsUser { IdUser = "76985", Username = "steven", FullName = "Steven", PasswordHash = "h", Role = "Cashier", Stsrc = true },
            new MsUser { IdUser = "66859", Username = "maria", FullName = "Maria", PasswordHash = "h", Role = "Cashier", Stsrc = true },
            new MsUser { IdUser = "55794", Username = "admin", FullName = "Admin", PasswordHash = "h", Role = "Admin", Stsrc = true }
        );

        _context.PaymentMethods.Add(new LtPaymentMethod { IdPaymentMethod = 1, MethodName = "Cash", Stsrc = true });

        // Seed transactions on 2026-04-09
        var targetDate = new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc);
        _context.Transactions.AddRange(
            new TrTransaction
            {
                IdTransaction = "tx-1",
                InvoiceNo = "76985090426001",
                IdUser = "76985", // steven
                IdPaymentMethod = 1,
                TotalPrice = 50000,
                TransactionDate = targetDate.AddHours(14), // 14:00
                Stsrc = true
            },
            new TrTransaction
            {
                IdTransaction = "tx-2",
                InvoiceNo = "76985090426002",
                IdUser = "76985", // steven
                IdPaymentMethod = 1,
                TotalPrice = 30000,
                TransactionDate = targetDate.AddHours(9), // 09:00
                Stsrc = true
            },
            new TrTransaction
            {
                IdTransaction = "tx-3",
                InvoiceNo = "66859090426001",
                IdUser = "66859", // maria
                IdPaymentMethod = 1,
                TotalPrice = 45000,
                TransactionDate = targetDate.AddHours(11), // 11:00
                Stsrc = true
            },
            new TrTransaction
            {
                IdTransaction = "tx-4",
                InvoiceNo = "76985100426001",
                IdUser = "76985",
                IdPaymentMethod = 1,
                TotalPrice = 10000,
                TransactionDate = targetDate.AddDays(1).AddHours(10), // Next day 2026-04-10
                Stsrc = true
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetHistory_CashierRole_ReturnsOnlyCashierOwnTransactions()
    {
        var targetDate = new DateTime(2026, 4, 9);
        var result = await _transactionService.GetHistoryAsync(targetDate, "76985", "Cashier", 1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal("steven", item.CashierUsername));
    }

    [Fact]
    public async Task GetHistory_AdminRole_ReturnsAllCashiersTransactions()
    {
        var targetDate = new DateTime(2026, 4, 9);
        var result = await _transactionService.GetHistoryAsync(targetDate, "55794", "Admin", 1, 20);

        // Should return all 3 transactions on 2026-04-09 (both Steven and Maria)
        Assert.Equal(3, result.TotalCount);
        Assert.Contains(result.Items, i => i.CashierUsername == "steven");
        Assert.Contains(result.Items, i => i.CashierUsername == "maria");
    }

    [Fact]
    public async Task GetHistory_OrderBy_AscendingByTransactionDate()
    {
        var targetDate = new DateTime(2026, 4, 9);
        var result = await _transactionService.GetHistoryAsync(targetDate, "55794", "Admin", 1, 20);

        Assert.Equal(3, result.Items.Count);
        // Sorted ascending: 09:00 (tx-2), 11:00 (tx-3), 14:00 (tx-1)
        Assert.Equal("tx-2", result.Items[0].IdTransaction);
        Assert.Equal("tx-3", result.Items[1].IdTransaction);
        Assert.Equal("tx-1", result.Items[2].IdTransaction);
    }

    [Fact]
    public async Task GetHistory_PageSizeExceeding20_ClampedTo20()
    {
        var targetDate = new DateTime(2026, 4, 9);
        var result = await _transactionService.GetHistoryAsync(targetDate, "55794", "Admin", 1, 100);

        Assert.Equal(20, result.PageSize);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
