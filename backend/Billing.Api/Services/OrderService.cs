using Billing.Api.Interfaces;
using Billing.Api.Models;
using Polly;

namespace Billing.Api.Services;

public class OrderService : IOrderService
{
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ResiliencePipeline<PaymentResult> _retryPipeline;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IIdempotencyStore idempotencyStore,
        IPaymentGatewayFactory gatewayFactory,
        ResiliencePipeline<PaymentResult> retryPipeline,
        ILogger<OrderService> logger)
    {
        _idempotencyStore = idempotencyStore;
        _gatewayFactory = gatewayFactory;
        _retryPipeline = retryPipeline;
        _logger = logger;
    }

    public async Task<(bool Success, Receipt? Receipt, string? Error)> ProcessOrderAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        if (_idempotencyStore.TryGetReceipt(order.OrderNumber, out var existing) && existing is not null)
        {
            _logger.LogInformation(
                "Returning cached receipt for order {OrderNumber} (idempotent request)",
                order.OrderNumber);

            return (true, existing, null);
        }

        var gateway = _gatewayFactory.GetGateway(order.PaymentGatewayId);
        if (gateway is null)
        {
            return (false, null, $"Unknown payment gateway: '{order.PaymentGatewayId}'.");
        }

        var paymentResult = await _retryPipeline.ExecuteAsync(
            async ct => await gateway.ProcessPaymentAsync(order, ct),
            cancellationToken);

        if (!paymentResult.Success)
        {
            return (false, null, paymentResult.ErrorMessage ?? "Payment failed.");
        }

        var receipt = new Receipt(
            order.OrderNumber,
            order.PayableAmount,
            DateTime.UtcNow,
            paymentResult.ConfirmationId);

        _idempotencyStore.SaveReceipt(order.OrderNumber, receipt);

        return (true, receipt, null);
    }
}
