using Billing.Api.Interfaces;

namespace Billing.Api.PaymentGateways;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly Dictionary<string, IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways.ToDictionary(g => g.GatewayId, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentGateway? GetGateway(string gatewayId) =>
        _gateways.GetValueOrDefault(gatewayId);
}
