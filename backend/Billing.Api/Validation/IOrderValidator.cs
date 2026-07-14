using Billing.Api.Models;

namespace Billing.Api.Validation;

public interface IOrderValidator
{
    (bool IsValid, IReadOnlyList<string> Errors) Validate(Order order);
}
