using BeeMart.Api.Data.Entities;
using BeeMart.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(BeeMartDbContext context, IPasswordHasher passwordHasher)
    {
        // Ensure database schema is created
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Payment Methods
        if (!await context.PaymentMethods.AnyAsync())
        {
            var paymentMethods = new List<LtPaymentMethod>
            {
                new() { IdPaymentMethod = 1, MethodName = "Cash", Stsrc = true, Datein = DateTime.UtcNow, Userin = "SYSTEM" },
                new() { IdPaymentMethod = 2, MethodName = "Debit", Stsrc = true, Datein = DateTime.UtcNow, Userin = "SYSTEM" },
                new() { IdPaymentMethod = 3, MethodName = "Credit", Stsrc = true, Datein = DateTime.UtcNow, Userin = "SYSTEM" }
            };
            await context.PaymentMethods.AddRangeAsync(paymentMethods);
            await context.SaveChangesAsync();
        }

        // 2. Seed Users
        if (!await context.Users.AnyAsync())
        {
            var defaultPasswordHash = passwordHasher.HashPassword("password");

            var users = new List<MsUser>
            {
                new()
                {
                    IdUser = "76985",
                    Username = "steven",
                    FullName = "Steven Anderson",
                    PasswordHash = defaultPasswordHash,
                    Role = "Cashier",
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdUser = "66859",
                    Username = "maria",
                    FullName = "Maria Chen",
                    PasswordHash = defaultPasswordHash,
                    Role = "Cashier",
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdUser = "55794",
                    Username = "admin",
                    FullName = "Administrator System",
                    PasswordHash = defaultPasswordHash,
                    Role = "Admin",
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdUser = "78886",
                    Username = "budi",
                    FullName = "Budi Santoso",
                    PasswordHash = defaultPasswordHash,
                    Role = "Cashier",
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdUser = "55356",
                    Username = "superadmin",
                    FullName = "Head Office Supervisor",
                    PasswordHash = defaultPasswordHash,
                    Role = "Admin",
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // 3. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var products = new List<MsProduct>
            {
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111101",
                    Name = "Apel Fuji Premium",
                    Weight = 1.00m,
                    Stock = 50,
                    Price = 45000.00m,
                    Discount = 5.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111102",
                    Name = "Apel Manalagi Segar",
                    Weight = 1.00m,
                    Stock = 35,
                    Price = 28000.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111103",
                    Name = "Susu UHT Full Cream 1L",
                    Weight = 1.00m,
                    Stock = 80,
                    Price = 21500.00m,
                    Discount = 10.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111104",
                    Name = "Susu Kental Manis 370g",
                    Weight = 0.37m,
                    Stock = 100,
                    Price = 12500.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111105",
                    Name = "Beras Pandan Wangi 5kg",
                    Weight = 5.00m,
                    Stock = 40,
                    Price = 78000.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111106",
                    Name = "Minyak Goreng Refill 2L",
                    Weight = 2.00m,
                    Stock = 60,
                    Price = 36000.00m,
                    Discount = 2.50m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111107",
                    Name = "Gula Pasir Kristal 1kg",
                    Weight = 1.00m,
                    Stock = 90,
                    Price = 17500.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111108",
                    Name = "Telur Ayam Negeri 1kg",
                    Weight = 1.00m,
                    Stock = 45,
                    Price = 31000.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111109",
                    Name = "Roti Tawar Gandum",
                    Weight = 0.40m,
                    Stock = 30,
                    Price = 18500.00m,
                    Discount = 5.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111110",
                    Name = "Kopi Bubuk Arabika 200g",
                    Weight = 0.20m,
                    Stock = 55,
                    Price = 34000.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111111",
                    Name = "Teh Celup Melati Kotak 25s",
                    Weight = 0.05m,
                    Stock = 120,
                    Price = 8500.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                },
                new()
                {
                    IdProduct = "11111111-1111-1111-1111-111111111112",
                    Name = "Mie Instan Goreng Spesial",
                    Weight = 0.08m,
                    Stock = 250,
                    Price = 3500.00m,
                    Discount = 0.00m,
                    Stsrc = true,
                    Datein = DateTime.UtcNow,
                    Userin = "SYSTEM"
                }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // 4. Seed initial transactions for history testing if empty
        if (!await context.Transactions.AnyAsync(t => t.InvoiceNo == "76985040926001"))
        {
            var t0 = new TrTransaction
            {
                IdTransaction = "22222222-2222-2222-2222-222222222200",
                InvoiceNo = "76985040926001",
                TransactionDate = new DateTime(2026, 9, 4, 10, 32, 46, DateTimeKind.Utc),
                IdUser = "76985",
                IdPaymentMethod = 1, // Cash
                TotalPrice = 89000.00m,
                Stsrc = true,
                Datein = new DateTime(2026, 9, 4, 10, 32, 46, DateTimeKind.Utc),
                Userin = "76985"
            };

            var t1 = new TrTransaction
            {
                IdTransaction = "22222222-2222-2222-2222-222222222201",
                InvoiceNo = "76985090426001",
                TransactionDate = new DateTime(2026, 4, 9, 10, 15, 30, DateTimeKind.Utc),
                IdUser = "76985",
                IdPaymentMethod = 1, // Cash
                TotalPrice = 89000.00m,
                Stsrc = true,
                Datein = new DateTime(2026, 4, 9, 10, 15, 30, DateTimeKind.Utc),
                Userin = "76985"
            };

            var t2 = new TrTransaction
            {
                IdTransaction = "22222222-2222-2222-2222-222222222202",
                InvoiceNo = "66859090426001",
                TransactionDate = new DateTime(2026, 4, 9, 11, 45, 10, DateTimeKind.Utc),
                IdUser = "66859",
                IdPaymentMethod = 2, // Debit
                TotalPrice = 78000.00m,
                Stsrc = true,
                Datein = new DateTime(2026, 4, 9, 11, 45, 10, DateTimeKind.Utc),
                Userin = "66859"
            };

            var td1 = new TrTransactionDetail
            {
                IdTransactionDetail = "33333333-3333-3333-3333-333333333301",
                IdTransaction = t1.IdTransaction,
                IdProduct = "11111111-1111-1111-1111-111111111101",
                Qty = 1,
                Price = 45000.00m,
                Discount = 5.00m,
                SubTotal = 42750.00m,
                Stsrc = true,
                Datein = t1.TransactionDate,
                Userin = "76985"
            };

            var td2 = new TrTransactionDetail
            {
                IdTransactionDetail = "33333333-3333-3333-3333-333333333302",
                IdTransaction = t1.IdTransaction,
                IdProduct = "11111111-1111-1111-1111-111111111103",
                Qty = 2,
                Price = 21500.00m,
                Discount = 10.00m,
                SubTotal = 38700.00m,
                Stsrc = true,
                Datein = t1.TransactionDate,
                Userin = "76985"
            };

            var td3 = new TrTransactionDetail
            {
                IdTransactionDetail = "33333333-3333-3333-3333-333333333303",
                IdTransaction = t1.IdTransaction,
                IdProduct = "11111111-1111-1111-1111-111111111111",
                Qty = 1,
                Price = 8500.00m,
                Discount = 0.00m,
                SubTotal = 8500.00m,
                Stsrc = true,
                Datein = t1.TransactionDate,
                Userin = "76985"
            };

            var td4 = new TrTransactionDetail
            {
                IdTransactionDetail = "33333333-3333-3333-3333-333333333304",
                IdTransaction = t2.IdTransaction,
                IdProduct = "11111111-1111-1111-1111-111111111105",
                Qty = 1,
                Price = 78000.00m,
                Discount = 0.00m,
                SubTotal = 78000.00m,
                Stsrc = true,
                Datein = t2.TransactionDate,
                Userin = "66859"
            };

            if (!await context.Transactions.AnyAsync(t => t.IdTransaction == t0.IdTransaction))
            {
                var td0a = new TrTransactionDetail
                {
                    IdTransactionDetail = "33333333-3333-3333-3333-333333333300",
                    IdTransaction = t0.IdTransaction,
                    IdProduct = "11111111-1111-1111-1111-111111111101",
                    Qty = 1,
                    Price = 45000.00m,
                    Discount = 5.00m,
                    SubTotal = 42750.00m,
                    Stsrc = true,
                    Datein = t0.TransactionDate,
                    Userin = "76985"
                };

                var td0b = new TrTransactionDetail
                {
                    IdTransactionDetail = "33333333-3333-3333-3333-333333333305",
                    IdTransaction = t0.IdTransaction,
                    IdProduct = "11111111-1111-1111-1111-111111111103",
                    Qty = 2,
                    Price = 21500.00m,
                    Discount = 10.00m,
                    SubTotal = 38700.00m,
                    Stsrc = true,
                    Datein = t0.TransactionDate,
                    Userin = "76985"
                };

                var td0c = new TrTransactionDetail
                {
                    IdTransactionDetail = "33333333-3333-3333-3333-333333333306",
                    IdTransaction = t0.IdTransaction,
                    IdProduct = "11111111-1111-1111-1111-111111111111",
                    Qty = 1,
                    Price = 8500.00m,
                    Discount = 0.00m,
                    SubTotal = 8500.00m,
                    Stsrc = true,
                    Datein = t0.TransactionDate,
                    Userin = "76985"
                };

                await context.Transactions.AddAsync(t0);
                await context.TransactionDetails.AddRangeAsync(td0a, td0b, td0c);
            }

            if (!await context.Transactions.AnyAsync(t => t.IdTransaction == t1.IdTransaction))
            {
                await context.Transactions.AddAsync(t1);
                await context.TransactionDetails.AddRangeAsync(td1, td2, td3);
            }

            if (!await context.Transactions.AnyAsync(t => t.IdTransaction == t2.IdTransaction))
            {
                await context.Transactions.AddAsync(t2);
                await context.TransactionDetails.AddAsync(td4);
            }

            await context.SaveChangesAsync();
        }
    }
}
