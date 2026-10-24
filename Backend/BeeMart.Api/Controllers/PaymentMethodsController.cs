using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var methods = await _paymentMethodService.GetPaymentMethodsAsync();
        return Ok(ApiResponse<List<PaymentMethodDto>>.Ok(methods));
    }
}
