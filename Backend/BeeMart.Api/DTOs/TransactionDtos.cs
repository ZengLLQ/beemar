using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class CreateTransactionRequest
{
    [Required]
    public string IdUser { get; set; } = string.Empty;

    [Required]
    public int IdPaymentMethod { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Keranjang belanja tidak boleh kosong.")]
    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}

public class CreateTransactionItemRequest
{
    [Required]
    public string IdProduct { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Qty minimal 1.")]
    public int Qty { get; set; }
}

public class TransactionResponse
{
    public string IdTransaction { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string IdUser { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public int IdPaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public List<TransactionDetailItemResponse> Items { get; set; } = new();
}

public class TransactionDetailItemResponse
{
    public string IdTransactionDetail { get; set; } = string.Empty;
    public string IdProduct { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
}

public class TransactionHistoryItemDto
{
    public string IdTransaction { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string CashierUsername { get; set; } = string.Empty;
    public string CashierFullName { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}

public class PaginatedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public List<T> Items { get; set; } = new();
}

public class NextInvoiceResponse
{
    public string InvoiceNo { get; set; } = string.Empty;
}
