using BeeMart.Api.Data;
using BeeMart.Api.Data.Entities;
using BeeMart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public interface ITransactionService
{
    Task<string> GenerateNextInvoiceNoAsync(string userId, DateTime? targetDate = null);
    Task<ApiResponse<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request);
    Task<PaginatedResponse<TransactionHistoryItemDto>> GetHistoryAsync(DateTime date, string? userId, string userRole, int page = 1, int pageSize = 20);
}

public class TransactionService : ITransactionService
{
    private readonly BeeMartDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(BeeMartDbContext context, ILogger<TransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateNextInvoiceNoAsync(string userId, DateTime? targetDate = null)
    {
        var date = targetDate ?? DateTime.UtcNow;
        var formattedUserId = (userId ?? "").Trim().PadLeft(5, '0');
        if (formattedUserId.Length > 5)
        {
            formattedUserId = formattedUserId.Substring(0, 5);
        }

        var datePart = date.ToString("ddMMyy");
        var prefix = $"{formattedUserId}{datePart}";

        // Find existing invoices starting with prefix
        var existingInvoices = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.InvoiceNo.StartsWith(prefix))
            .Select(t => t.InvoiceNo)
            .ToListAsync();

        int maxRunning = 0;
        foreach (var inv in existingInvoices)
        {
            if (inv.Length >= prefix.Length + 3)
            {
                var suffix = inv.Substring(prefix.Length, 3);
                if (int.TryParse(suffix, out int num) && num > maxRunning)
                {
                    maxRunning = num;
                }
            }
        }

        int nextRunning = maxRunning + 1;
        var runningPart = nextRunning.ToString("D3");

        return $"{prefix}{runningPart}";
    }

    public async Task<ApiResponse<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdUser))
        {
            return ApiResponse<TransactionResponse>.Fail("User ID tidak valid.");
        }

        if (request.Items == null || !request.Items.Any())
        {
            return ApiResponse<TransactionResponse>.Fail("Keranjang belanja tidak boleh kosong.");
        }

        // Validate User
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.IdUser == request.IdUser && u.Stsrc);
        if (user == null)
        {
            return ApiResponse<TransactionResponse>.Fail("User kasir tidak ditemukan atau tidak aktif.");
        }

        // Validate Payment Method
        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(p => p.IdPaymentMethod == request.IdPaymentMethod && p.Stsrc);
        if (paymentMethod == null)
        {
            return ApiResponse<TransactionResponse>.Fail("Metode pembayaran tidak valid.");
        }

        // Validate products & stock
        var productIds = request.Items.Select(i => i.IdProduct).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.IdProduct) && p.Stsrc)
            .ToDictionaryAsync(p => p.IdProduct);

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.IdProduct, out var product))
            {
                return ApiResponse<TransactionResponse>.Fail($"Produk tidak ditemukan.");
            }

            if (item.Qty <= 0)
            {
                return ApiResponse<TransactionResponse>.Fail("Jumlah barang harus lebih dari 0.");
            }

            // PDF Requirement: "Jumlah barang yang dimasukkan melebihi sisa stok."
            if (item.Qty > product.Stock)
            {
                return ApiResponse<TransactionResponse>.Fail("Jumlah barang yang dimasukkan melebihi sisa stok.");
            }
        }

        // Generate Invoice Number
        var invoiceNo = await GenerateNextInvoiceNoAsync(user.IdUser, DateTime.UtcNow);

        var transaction = new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = invoiceNo,
            TransactionDate = DateTime.UtcNow,
            IdUser = user.IdUser,
            IdPaymentMethod = paymentMethod.IdPaymentMethod,
            TotalPrice = 0,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = user.IdUser
        };

        decimal totalPrice = 0;
        var detailResponses = new List<TransactionDetailItemResponse>();

        foreach (var item in request.Items)
        {
            var product = products[item.IdProduct];

            // Deduct stock
            product.Stock -= item.Qty;
            product.Dateup = DateTime.UtcNow;
            product.Userup = user.IdUser;

            var discountedUnitPrice = product.Price - (product.Price * (product.Discount / 100m));
            var subTotal = Math.Round(discountedUnitPrice * item.Qty, 2);
            totalPrice += subTotal;

            var detail = new TrTransactionDetail
            {
                IdTransactionDetail = Guid.NewGuid().ToString(),
                IdTransaction = transaction.IdTransaction,
                IdProduct = product.IdProduct,
                Qty = item.Qty,
                Price = product.Price,
                Discount = product.Discount,
                SubTotal = subTotal,
                Stsrc = true,
                Datein = DateTime.UtcNow,
                Userin = user.IdUser
            };

            transaction.TransactionDetails.Add(detail);

            detailResponses.Add(new TransactionDetailItemResponse
            {
                IdTransactionDetail = detail.IdTransactionDetail,
                IdProduct = product.IdProduct,
                ProductName = product.Name,
                Qty = item.Qty,
                Price = product.Price,
                Discount = product.Discount,
                SubTotal = subTotal
            });
        }

        transaction.TotalPrice = Math.Round(totalPrice, 2);

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        var response = new TransactionResponse
        {
            IdTransaction = transaction.IdTransaction,
            InvoiceNo = transaction.InvoiceNo,
            TransactionDate = transaction.TransactionDate,
            IdUser = user.IdUser,
            CashierName = user.Username,
            IdPaymentMethod = paymentMethod.IdPaymentMethod,
            PaymentMethodName = paymentMethod.MethodName,
            TotalPrice = transaction.TotalPrice,
            Items = detailResponses
        };

        return ApiResponse<TransactionResponse>.Ok(response, "Transaksi berhasil diproses.");
    }

    public async Task<PaginatedResponse<TransactionHistoryItemDto>> GetHistoryAsync(
        DateTime date,
        string? userId,
        string userRole,
        int page = 1,
        int pageSize = 20)
    {
        // Enforce max 20 per page as per specification
        pageSize = Math.Clamp(pageSize, 1, 20);
        if (page < 1) page = 1;

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.PaymentMethod)
            .Include(t => t.TransactionDetails)
            .Where(t => t.Stsrc && t.TransactionDate >= startOfDay && t.TransactionDate < endOfDay);

        // Role-based filtering:
        // Cashier only sees their own transactions
        // Admin sees all cashiers
        bool isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && !string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(t => t.IdUser == userId);
        }

        var totalCount = await query.CountAsync();

        // Order By Ascending based on Tanggal and Jam transaksi (Specification requirement)
        var items = await query
            .OrderBy(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionHistoryItemDto
            {
                IdTransaction = t.IdTransaction,
                InvoiceNo = t.InvoiceNo,
                TransactionDate = t.TransactionDate,
                CashierUsername = t.User != null ? t.User.Username : "",
                CashierFullName = t.User != null ? t.User.FullName : "",
                TotalItems = t.TransactionDetails.Sum(d => d.Qty),
                PaymentMethodName = t.PaymentMethod != null ? t.PaymentMethod.MethodName : "",
                TotalPrice = t.TotalPrice
            })
            .ToListAsync();

        return new PaginatedResponse<TransactionHistoryItemDto>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
}
