using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("next-invoice")]
    public async Task<IActionResult> GetNextInvoice([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(ApiResponse<NextInvoiceResponse>.Fail("userId wajib diisi."));
        }

        var invoiceNo = await _transactionService.GenerateNextInvoiceNoAsync(userId);
        return Ok(ApiResponse<NextInvoiceResponse>.Ok(new NextInvoiceResponse { InvoiceNo = invoiceNo }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<TransactionResponse>.Fail(errors.FirstOrDefault() ?? "Data transaksi tidak valid.", errors));
        }

        var result = await _transactionService.CreateTransactionAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string date,
        [FromQuery] string? userId,
        [FromQuery] string role = "Cashier",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
        {
            return BadRequest(ApiResponse<PaginatedResponse<TransactionHistoryItemDto>>.Fail("Format tanggal tidak valid."));
        }

        var response = await _transactionService.GetHistoryAsync(parsedDate, userId, role, page, pageSize);
        return Ok(ApiResponse<PaginatedResponse<TransactionHistoryItemDto>>.Ok(response));
    }
}
