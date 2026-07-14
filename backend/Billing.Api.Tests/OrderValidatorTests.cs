using Billing.Api.Models;
using Billing.Api.PaymentGateways;
using Billing.Api.Validation;
using FluentAssertions;

namespace Billing.Api.Tests;

public class OrderValidatorTests
{
    private readonly OrderValidator _validator = new();

    [Fact]
    public void Validate_WithValidOrder_ReturnsNoErrors()
    {
        var order = new Order("ORD-1", "user-1", 10m, "stripe", null);

        var (isValid, errors) = _validator.Validate(order);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "user-1", 10, "stripe", "Order number is required.")]
    [InlineData("ORD-1", "", 10, "stripe", "User ID is required.")]
    [InlineData("ORD-1", "user-1", 0, "stripe", "Payable amount must be greater than zero.")]
    [InlineData("ORD-1", "user-1", 10, "", "Payment gateway ID is required.")]
    public void Validate_WithInvalidFields_ReturnsExpectedError(
        string orderNumber,
        string userId,
        decimal amount,
        string gatewayId,
        string expectedError)
    {
        var order = new Order(orderNumber, userId, amount, gatewayId, null);

        var (isValid, errors) = _validator.Validate(order);

        isValid.Should().BeFalse();
        errors.Should().Contain(expectedError);
    }
}
