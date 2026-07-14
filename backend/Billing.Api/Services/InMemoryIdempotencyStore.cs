using System.Collections.Concurrent;
using Billing.Api.Interfaces;
using Billing.Api.Models;

namespace Billing.Api.Services;

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Receipt> _receipts = new();

    public bool TryGetReceipt(string orderNumber, out Receipt? receipt)
    {
        if (_receipts.TryGetValue(orderNumber, out var cached))
        {
            receipt = cached;
            return true;
        }

        receipt = null;
        return false;
    }

    public void SaveReceipt(string orderNumber, Receipt receipt)
    {
        _receipts[orderNumber] = receipt;
    }
}
