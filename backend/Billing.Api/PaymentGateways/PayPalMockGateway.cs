using Billing.Api.Interfaces;
using Billing.Api.Models;

namespace Billing.Api.PaymentGateways;

public class PayPalMockGateway : IPaymentGateway
{
    public string GatewayId => "paypal";

    public Task<PaymentResult> ProcessPaymentAsync(Order order, CancellationToken ct)
    {
        if (order.PayableAmount <= 0)
        {
            return Task.FromResult(new PaymentResult(
                false,
                string.Empty,
                "PayPal mock: amount must be positive."));
        }

        if (order.PayableAmount % 1m == 0.99m)
        {
            return Task.FromResult(new PaymentResult(
                false,
                string.Empty,
                "PayPal mock declined this payment amount.",
                IsTransient: false));
        }

        var confirmationId = $"paypal_{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentResult(true, confirmationId, null));
    }
}
