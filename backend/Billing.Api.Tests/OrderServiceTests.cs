using Billing.Api.Interfaces;
using Billing.Api.Models;
using Billing.Api.PaymentGateways;
using Billing.Api.Services;
using Billing.Api.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polly;

namespace Billing.Api.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task ProcessOrder_WithSameOrderNumber_ProcessesPaymentOnlyOnce()
    {
        var store = new InMemoryIdempotencyStore();
        var gateway = CreateGatewayMock("stripe", new PaymentResult(true, "CONF-123", null));
        var factory = CreateFactoryMock(gateway.Object);
        var service = CreateService(store, factory.Object);

        var order = new Order("ORD-001", "user-1", 10m, "stripe", null);

        var first = await service.ProcessOrderAsync(order);
        var second = await service.ProcessOrderAsync(order);

        first.Success.Should().BeTrue();
        second.Success.Should().BeTrue();
        second.Receipt.Should().BeEquivalentTo(first.Receipt);
        gateway.Verify(g => g.ProcessPaymentAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOrder_WhenPaymentFails_ReturnsError()
    {
        var store = new InMemoryIdempotencyStore();
        var gateway = CreateGatewayMock("paypal", new PaymentResult(false, "", "Declined", false));
        var factory = CreateFactoryMock(gateway.Object);
        var service = CreateService(store, factory.Object);
        var order = new Order("ORD-002", "user-1", 9.99m, "paypal", null);

        var result = await service.ProcessOrderAsync(order);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Declined");
        result.Receipt.Should().BeNull();
        store.TryGetReceipt(order.OrderNumber, out _).Should().BeFalse();
    }

    [Fact]
    public async Task ProcessOrder_WithUnknownGateway_ReturnsError()
    {
        var store = new InMemoryIdempotencyStore();
        var factory = new Mock<IPaymentGatewayFactory>();
        factory.Setup(f => f.GetGateway("unknown")).Returns((IPaymentGateway?)null);
        var service = CreateService(store, factory.Object);

        var result = await service.ProcessOrderAsync(
            new Order("ORD-003", "user-1", 10m, "unknown", null));

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Unknown payment gateway");
    }

    [Fact]
    public async Task ProcessOrder_WhenSuccessful_SavesReceipt()
    {
        var store = new InMemoryIdempotencyStore();
        var gateway = CreateGatewayMock("stripe", new PaymentResult(true, "CONF-999", null));
        var factory = CreateFactoryMock(gateway.Object);
        var service = CreateService(store, factory.Object);
        var order = new Order("ORD-004", "user-1", 25m, "stripe", "Books");

        var result = await service.ProcessOrderAsync(order);

        result.Success.Should().BeTrue();
        store.TryGetReceipt(order.OrderNumber, out var receipt).Should().BeTrue();
        receipt!.PaymentConfirmation.Should().Be("CONF-999");
    }

    private static OrderService CreateService(IIdempotencyStore store, IPaymentGatewayFactory factory)
    {
        var pipeline = new ResiliencePipelineBuilder<PaymentResult>().Build();
        return new OrderService(store, factory, pipeline, NullLogger<OrderService>.Instance);
    }

    private static Mock<IPaymentGateway> CreateGatewayMock(string gatewayId, PaymentResult result)
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.GatewayId).Returns(gatewayId);
        gateway.Setup(g => g.ProcessPaymentAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return gateway;
    }

    private static Mock<IPaymentGatewayFactory> CreateFactoryMock(IPaymentGateway gateway)
    {
        var factory = new Mock<IPaymentGatewayFactory>();
        factory.Setup(f => f.GetGateway(gateway.GatewayId)).Returns(gateway);
        return factory;
    }
}
