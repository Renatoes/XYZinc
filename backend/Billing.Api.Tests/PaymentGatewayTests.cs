using Billing.Api.Interfaces;
using Billing.Api.Models;
using Billing.Api.PaymentGateways;
using FluentAssertions;

namespace Billing.Api.Tests;

public class PaymentGatewayTests
{
    [Fact]
    public async Task StripeMockGateway_SucceedsForNormalAmount()
    {
        var gateway = new StripeMockGateway();
        var order = new Order("ORD-S1", "user-1", 10m, "stripe", null);

        var result = await gateway.ProcessPaymentAsync(order, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ConfirmationId.Should().StartWith("stripe_");
    }

    [Fact]
    public async Task PayPalMockGateway_FailsWhenAmountEndsWith99()
    {
        var gateway = new PayPalMockGateway();
        var order = new Order("ORD-P1", "user-1", 9.99m, "paypal", null);

        var result = await gateway.ProcessPaymentAsync(order, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.IsTransient.Should().BeFalse();
    }

    [Fact]
    public void PaymentGatewayFactory_ReturnsRegisteredGateway()
    {
        IPaymentGateway[] gateways = [new StripeMockGateway(), new PayPalMockGateway()];
        var factory = new PaymentGatewayFactory(gateways);

        factory.GetGateway("stripe").Should().BeOfType<StripeMockGateway>();
        factory.GetGateway("PAYPAL").Should().BeOfType<PayPalMockGateway>();
        factory.GetGateway("missing").Should().BeNull();
    }
}
