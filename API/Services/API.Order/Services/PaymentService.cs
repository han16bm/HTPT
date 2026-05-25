using API.Order.Interfaces;
using API.Order.Models.Commands;
using API.Order.Models.DTOs;
using netcore.Commons.Exceptions;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Order.Services;

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
        var order = await _uow.Orders.FirstOrDefaultAsync(o => o.Id == (decimal)request.OrderId, ct);
        if (order == null)
        {
            throw new NotFoundException("Đơn hàng", request.OrderId);
        }

        var paymentCode = "TT" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
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

        if (payment == null)
        {
            return null;
        }

        var orderCode = order != null ? order.OrderCode : string.Empty;
        return MapToDto(payment, orderCode);
    }

    private static PaymentDto MapToDto(Payment payment, string orderCode)
    {
        return new PaymentDto
        {
            Id = (long)payment.Id,
            OrderId = (long)payment.OrderId,
            OrderCode = orderCode,
            PaymentMethod = payment.Method,
            Status = payment.Status,
            Amount = payment.Amount,
            TransactionCode = payment.TransactionRef,
            Notes = payment.Note,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
        };
    }
}
