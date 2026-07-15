using Billing.Api.Models;
namespace Billing.Api.Interfaces;

public interface IIdempotencyStore
{
    bool TryGetReceipt(string orderNumber, out Receipt? receipt);
    void SaveReceipt(string orderNumber, Receipt receipt);
}
