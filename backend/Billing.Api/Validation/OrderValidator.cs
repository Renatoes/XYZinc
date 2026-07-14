using Billing.Api.Models;

namespace Billing.Api.Validation;

public class OrderValidator : IOrderValidator
{
    public (bool IsValid, IReadOnlyList<string> Errors) Validate(Order order)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            errors.Add("Order number is required.");
        }

        if (string.IsNullOrWhiteSpace(order.UserId))
        {
            errors.Add("User ID is required.");
        }

        if (order.PayableAmount <= 0)
        {
            errors.Add("Payable amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(order.PaymentGatewayId))
        {
            errors.Add("Payment gateway ID is required.");
        }

        return (errors.Count == 0, errors);
    }
}
