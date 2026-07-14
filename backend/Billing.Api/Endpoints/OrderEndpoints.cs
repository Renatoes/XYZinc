using Billing.Api.Interfaces;
using Billing.Api.Models;
using Billing.Api.Validation;

namespace Billing.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/api/orders", SubmitOrder)
            .WithName("SubmitOrder")
            .WithTags("Orders")
            .Produces<Receipt>(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status400BadRequest);

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("HealthCheck")
            .WithTags("Health");
    }

    private static async Task<IResult> SubmitOrder(
        Order order,
        IOrderService orderService,
        IOrderValidator validator,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        var (isValid, errors) = validator.Validate(order);
        if (!isValid)
        {
            logger.LogWarning(
                "Order validation failed for {OrderNumber}: {Errors}",
                order.OrderNumber,
                string.Join("; ", errors));

            return Results.BadRequest(new Error(
                "Validation failed.",
                order.OrderNumber,
                errors));
        }

        logger.LogInformation(
            "Processing order {OrderNumber} for user {UserId} via {Gateway}",
            order.OrderNumber,
            order.UserId,
            order.PaymentGatewayId);

        var (success, receipt, error) = await orderService.ProcessOrderAsync(order, cancellationToken);

        if (success)
        {
            logger.LogInformation(
                "Order {OrderNumber} paid successfully with confirmation {Confirmation}",
                order.OrderNumber,
                receipt!.PaymentConfirmation);

            return Results.Ok(receipt);
        }

        logger.LogWarning("Order {OrderNumber} payment failed: {Error}", order.OrderNumber, error);

        return Results.BadRequest(new Error(error!, order.OrderNumber));
    }
}
