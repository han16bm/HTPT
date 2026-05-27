using MassTransit;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Messages.Events;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Order.Consumers;

public class PaymentProcessConsumer : IConsumer<PaymentProcessRequested>
{
    private const int DefaultCodSuccessRatePercent = 95;
    private const int DefaultBankTransferSuccessRatePercent = 90;
    private const int DefaultProcessingDelaySeconds = 5;
    private const int MaxProcessingDelaySeconds = 30;

    private readonly ILogger<PaymentProcessConsumer> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;

    public PaymentProcessConsumer(
        ILogger<PaymentProcessConsumer> logger,
        IUnitOfWork uow,
        IConfiguration configuration)
    {
        _logger = logger;
        _uow = uow;
        _configuration = configuration;
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

        var delaySeconds = GetIntSetting(
            "PaymentMock:ProcessingDelaySeconds",
            DefaultProcessingDelaySeconds,
            0,
            MaxProcessingDelaySeconds);

        if (delaySeconds > 0)
        {
            _logger.LogInformation("Mock payment delay {DelaySeconds}s for order {OrderCode}",
                delaySeconds, msg.OrderCode);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), context.CancellationToken);
        }

        var currentOrderStatus = await _uow.Orders.Query()
            .AsNoTracking()
            .Where(o => o.Id == order.Id)
            .Select(o => o.OrderStatus)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (string.Equals(currentOrderStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
        {
            const string reason = "Đơn hàng đã bị hủy trước khi xác minh thanh toán";

            if (existingPayment == null)
            {
                var cancelledPayment = new Payment
                {
                    OrderId = order.Id,
                    PaymentCode = "TT" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    Method = msg.PaymentMethod,
                    Status = "FAILED",
                    Amount = msg.Amount,
                    TransactionRef = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _uow.Payments.AddAsync(cancelledPayment, context.CancellationToken);
            }
            else
            {
                existingPayment.Status = "FAILED";
                existingPayment.TransactionRef = string.Empty;
                existingPayment.PaidAt = null;
                existingPayment.UpdatedAt = DateTime.UtcNow;
                _uow.Payments.Update(existingPayment);
            }

            await _uow.SaveChangesAsync(context.CancellationToken);

            await context.Publish(new PaymentFailedEvent
            {
                OrderCode = msg.OrderCode,
                Reason = reason,
            });

            _logger.LogInformation("Skip payment processing for cancelled order {OrderCode}", msg.OrderCode);
            return;
        }

        var result = SimulateGateway(
            msg.PaymentMethod,
            msg.Amount,
            GetIntSetting("PaymentMock:CodSuccessRatePercent", DefaultCodSuccessRatePercent, 0, 100),
            GetIntSetting("PaymentMock:BankTransferSuccessRatePercent", DefaultBankTransferSuccessRatePercent, 0, 100));

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

        if (!result.Success)
        {
            order.OrderStatus = "CANCELLED";
            order.PaymentStatus = "FAILED";
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(order.Note))
            {
                order.Note = result.Reason;
            }
            else if (!order.Note.Contains(result.Reason, StringComparison.OrdinalIgnoreCase))
            {
                order.Note = order.Note + " | Payment: " + result.Reason;
            }

            _uow.Orders.Update(order);
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

    // FE hiện chỉ có COD và BANK_TRANSFER. Tỷ lệ và delay lấy từ PaymentMock trong cấu hình.
    private static GatewayResult SimulateGateway(
        string method,
        decimal amount,
        int codSuccessRatePercent,
        int bankTransferSuccessRatePercent)
    {
        var normalizedMethod = (method ?? string.Empty).ToUpperInvariant();
        var transactionRef = ("SIM-" + Guid.NewGuid().ToString("N")).Substring(0, 16);

        if (normalizedMethod == "COD" || normalizedMethod == "CASH")
        {
            return BuildResultByRate(
                codSuccessRatePercent,
                "PENDING",
                transactionRef,
                "COD bị từ chối bởi cổng thanh toán mock");
        }

        if (normalizedMethod == "BANK_TRANSFER")
        {
            return BuildResultByRate(
                bankTransferSuccessRatePercent,
                "AWAITING",
                transactionRef,
                "Chuyển khoản bị từ chối bởi cổng thanh toán mock");
        }

        return new GatewayResult(false, "FAILED", transactionRef, "Phương thức không hỗ trợ: " + method);
    }

    private static GatewayResult BuildResultByRate(
        int successRatePercent,
        string successStatus,
        string transactionRef,
        string failureReason)
    {
        var normalizedRate = Math.Clamp(successRatePercent, 0, 100);
        if (normalizedRate == 100 || Random.Shared.Next(100) < normalizedRate)
        {
            return new GatewayResult(true, successStatus, transactionRef, string.Empty);
        }

        return new GatewayResult(false, "FAILED", transactionRef, failureReason);
    }

    private int GetIntSetting(string key, int fallback, int min, int max)
    {
        var rawValue = _configuration[key];
        if (!int.TryParse(rawValue, out var value))
        {
            value = fallback;
        }

        return Math.Clamp(value, min, max);
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
