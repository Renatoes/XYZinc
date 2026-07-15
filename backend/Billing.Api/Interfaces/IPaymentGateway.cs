using Billing.Api.Models;

namespace Billing.Api.Interfaces;

public interface IPaymentGateway
{
    string GatewayId { get; }
    Task<PaymentResult> ProcessPaymentAsync(Order order, CancellationToken ct);
}

public record PaymentResult(
    bool Success,
    string ConfirmationId,
    string? ErrorMessage,
    bool IsTransient = false);