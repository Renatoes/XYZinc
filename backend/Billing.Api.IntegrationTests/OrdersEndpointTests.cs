using System.Net;
using System.Net.Http.Json;
using Billing.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Billing.Api.IntegrationTests;

public class OrdersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostOrder_ReturnsReceipt_WhenStripePaymentSucceeds()
    {
        var order = new
        {
            orderNumber = $"INT-{Guid.NewGuid():N}",
            userId = "user-integration",
            payableAmount = 20.00m,
            paymentGatewayId = "stripe",
            description = "Integration test order"
        };

        var response = await _client.PostAsJsonAsync("/api/orders", order);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var receipt = await response.Content.ReadFromJsonAsync<Receipt>();
        receipt!.OrderNumber.Should().Be(order.orderNumber);
        receipt.Amount.Should().Be(order.payableAmount);
        receipt.PaymentConfirmation.Should().StartWith("stripe_");
    }

    [Fact]
    public async Task PostOrder_ReturnsBadRequest_WhenPayPalDeclines()
    {
        var order = new
        {
            orderNumber = $"INT-{Guid.NewGuid():N}",
            userId = "user-integration",
            payableAmount = 19.99m,
            paymentGatewayId = "paypal",
            description = (string?)null
        };

        var response = await _client.PostAsJsonAsync("/api/orders", order);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<Error>();
        error!.Message.Should().Contain("declined");
    }

    [Fact]
    public async Task PostOrder_IsIdempotent_ForSameOrderNumber()
    {
        var orderNumber = $"INT-{Guid.NewGuid():N}";
        var order = new
        {
            orderNumber,
            userId = "user-integration",
            payableAmount = 15.00m,
            paymentGatewayId = "stripe"
        };

        var first = await _client.PostAsJsonAsync("/api/orders", order);
        var second = await _client.PostAsJsonAsync("/api/orders", order);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstReceipt = await first.Content.ReadFromJsonAsync<Receipt>();
        var secondReceipt = await second.Content.ReadFromJsonAsync<Receipt>();
        secondReceipt.Should().BeEquivalentTo(firstReceipt);
    }

    [Fact]
    public async Task PostOrder_ReturnsBadRequest_WhenValidationFails()
    {
        var order = new
        {
            orderNumber = "",
            userId = "user-integration",
            payableAmount = -1m,
            paymentGatewayId = "stripe"
        };

        var response = await _client.PostAsJsonAsync("/api/orders", order);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<Error>();
        error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
