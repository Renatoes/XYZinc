using System.Collections.Concurrent;
using Billing.Api.Interfaces;
using Billing.Api.Models;

namespace Billing.Api.PaymentGateways;

public class StripeMockGateway : IPaymentGateway
{
    private static readonly ConcurrentDictionary<string, int> TransientAttempts = new();

    public string GatewayId => "stripe";

    public Task<PaymentResult> ProcessPaymentAsync(Order order, CancellationToken ct)
    {
        // Demo rule: amount 12.34 simulates transient Stripe outages that succeed after retries.
        if (order.PayableAmount == 12.34m)
        {
            var attempt = TransientAttempts.AddOrUpdate(order.OrderNumber, 1, (_, count) => count + 1);
            if (attempt < 3)
            {
                return Task.FromResult(new PaymentResult(
                    false,
                    string.Empty,
                    "Stripe mock: temporary network error.",
                    IsTransient: true));
            }
        }

        var confirmationId = $"stripe_{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentResult(true, confirmationId, null));
    }
}
