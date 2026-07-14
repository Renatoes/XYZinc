namespace Billing.Api.Models;

public record Error(
    string Message,
    string? OrderNumber = null,
    IReadOnlyList<string>? Details = null);