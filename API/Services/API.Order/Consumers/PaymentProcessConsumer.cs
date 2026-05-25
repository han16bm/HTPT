using MassTransit;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Messages.Events;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Order.Consumers;

public class PaymentProcessConsumer : IConsumer<PaymentProcessRequested>
{
    private readonly ILogger<PaymentProcessConsumer> _logger;
    private readonly IUnitOfWork _uow;

    public PaymentProcessConsumer(ILogger<PaymentProcessConsumer> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task Consume(ConsumeContext<PaymentProcessRequested> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Saga process payment: {OrderCode} method={Method} amount={Amount}",
            msg.OrderCode, msg.PaymentMethod, msg.Amount);

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderCode == msg.OrderCode, context.CancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy order {OrderCode} khi xử lý payment", msg.OrderCode);
            await context.Publish(new PaymentFailedEvent
            {
                OrderCode = msg.OrderCode,
                Reason = "Không tìm thấy đơn hàng tương ứng",
            });
            return;
        }

        var existingPayment = await _uow.Payments.Query()
            .FirstOrDefaultAsync(p => p.OrderId == order.Id, context.CancellationToken);

        if (existingPayment != null && IsTerminal(existingPayment.Status))
        {
            _logger.LogInformation("Payment {OrderCode} đã ở trạng thái cuối ({Status}), bỏ qua xử lý",
                msg.OrderCode, existingPayment.Status);
            return;
        }

        var result = SimulateGateway(msg.PaymentMethod, msg.Amount);

        if (existingPayment == null)
        {
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentCode = "TT" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                Method = msg.PaymentMethod,
                Status = result.Status,
                Amount = msg.Amount,
                TransactionRef = result.TransactionRef,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PaidAt = result.Success ? DateTime.UtcNow : null,
            };
            await _uow.Payments.AddAsync(payment, context.CancellationToken);
        }
        else
        {
            existingPayment.Status = result.Status;
            existingPayment.TransactionRef = result.TransactionRef;
            existingPayment.PaidAt = result.Success ? DateTime.UtcNow : null;
            existingPayment.UpdatedAt = DateTime.UtcNow;
            _uow.Payments.Update(existingPayment);
        }

        await _uow.SaveChangesAsync(context.CancellationToken);

        if (result.Success)
        {
            await context.Publish(new PaymentProcessedEvent
            {
                OrderCode = msg.OrderCode,
                TransactionRef = result.TransactionRef,
                PaymentStatus = result.Status,
                ProcessedAt = DateTime.UtcNow,
            });
        }
        else
        {
            await context.Publish(new PaymentFailedEvent
            {
                OrderCode = msg.OrderCode,
                Reason = result.Reason,
            });
        }
    }

    private static bool IsTerminal(string status)
    {
        return status == "PAID" || status == "FAILED" || status == "AWAITING";
    }

    // Mock gateway: COD/CASH luôn pass, VNPAY/MOMO 90% pass, BANK_TRANSFER trả AWAITING
    private static GatewayResult SimulateGateway(string method, decimal amount)
    {
        var normalizedMethod = (method ?? string.Empty).ToUpperInvariant();
        var transactionRef = ("SIM-" + Guid.NewGuid().ToString("N")).Substring(0, 16);

        if (normalizedMethod == "COD" || normalizedMethod == "CASH")
        {
            return new GatewayResult(true, "PENDING", transactionRef, string.Empty);
        }

        if (normalizedMethod == "BANK_TRANSFER")
        {
            return new GatewayResult(true, "AWAITING", transactionRef, string.Empty);
        }

        if (normalizedMethod == "VNPAY" || normalizedMethod == "MOMO")
        {
            var randomScore = Random.Shared.Next(100);
            if (randomScore < 90)
            {
                return new GatewayResult(true, "PAID", transactionRef, string.Empty);
            }
            return new GatewayResult(false, "FAILED", transactionRef, "Gateway từ chối giao dịch (mock)");
        }

        return new GatewayResult(false, "FAILED", transactionRef, "Phương thức không hỗ trợ: " + method);
    }

    private sealed class GatewayResult
    {
        public bool Success { get; }
        public string Status { get; }
        public string TransactionRef { get; }
        public string Reason { get; }

        public GatewayResult(bool success, string status, string transactionRef, string reason)
        {
            Success = success;
            Status = status;
            TransactionRef = transactionRef;
            Reason = reason;
        }
    }
}
