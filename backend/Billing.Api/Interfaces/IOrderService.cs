using Billing.Api.Models;

namespace Billing.Api.Interfaces;

public interface IOrderService
{
    Task<(bool Success, Receipt? Receipt, string? Error)> ProcessOrderAsync(
        Order order,
        CancellationToken cancellationToken = default);
}
