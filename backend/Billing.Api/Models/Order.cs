namespace Billing.Api.Models;

public record Order(
    string OrderNumber,
    string UserId,
    decimal PayableAmount,
    string PaymentGatewayId,
    string? Description
);
