namespace Billing.Api.Interfaces;

public interface IPaymentGatewayFactory
{
    IPaymentGateway? GetGateway(string gatewayId);
}
