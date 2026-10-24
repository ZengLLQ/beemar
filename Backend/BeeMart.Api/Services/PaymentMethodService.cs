using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public interface IPaymentMethodService
{
    Task<List<PaymentMethodDto>> GetPaymentMethodsAsync();
}

public class PaymentMethodService : IPaymentMethodService
{
    private readonly BeeMartDbContext _context;

    public PaymentMethodService(BeeMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentMethodDto>> GetPaymentMethodsAsync()
    {
        return await _context.PaymentMethods
            .AsNoTracking()
            .Where(p => p.Stsrc)
            .OrderBy(p => p.IdPaymentMethod)
            .Select(p => new PaymentMethodDto
            {
                IdPaymentMethod = p.IdPaymentMethod,
                MethodName = p.MethodName
            })
            .ToListAsync();
    }
}
