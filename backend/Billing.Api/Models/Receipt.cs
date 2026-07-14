namespace Billing.Api.Models;

public record Receipt(
    string OrderNumber,
    decimal Amount,
    DateTime Timestamp,
    string PaymentConfirmation
);
