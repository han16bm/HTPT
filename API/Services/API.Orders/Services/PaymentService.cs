using API.Orders.Interfaces;
using API.Orders.Models.Commands;
using API.Orders.Models.DTOs;
using netcore.Commons.Exceptions;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Orders.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IUnitOfWork uow, ILogger<PaymentService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        var order = await _uow.Orders.FirstOrDefaultAsync(o => o.Id == (decimal)request.OrderId, ct)
            ?? throw new NotFoundException("Đơn hàng", request.OrderId);

        var paymentCode = $"TT{DateTime.UtcNow:yyyyMMddHHmmss}";
        var payment = new Payment
        {
            OrderId = order.Id,
            PaymentCode = paymentCode,
            Method = request.PaymentMethod,
            Status = "PENDING",
            Amount = order.TotalAmount,
            TransactionRef = request.TransactionCode,
            Note = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _uow.Payments.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Payment created: {PaymentCode} for order {OrderId}", paymentCode, request.OrderId);
        return MapToDto(payment, order.OrderCode);
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.FirstOrDefaultAsync(o => o.Id == (decimal)orderId, ct);
        var payment = await _uow.Payments.FirstOrDefaultAsync(p => p.OrderId == (decimal)orderId, ct);

        if (payment is null) return null;
        return MapToDto(payment, order?.OrderCode ?? string.Empty);
    }

    private static PaymentDto MapToDto(Payment p, string orderCode) => new()
    {
        Id = (long)p.Id,
        OrderId = (long)p.OrderId,
        OrderCode = orderCode,
        PaymentMethod = p.Method,
        Status = p.Status,
        Amount = p.Amount,
        TransactionCode = p.TransactionRef,
        Notes = p.Note,
        PaidAt = p.PaidAt,
        CreatedAt = p.CreatedAt,
    };
}
