using Billing.Api.Interfaces;
using Billing.Api.PaymentGateways;
using Billing.Api.Services;
using Billing.Api.Validation;
using Polly;
using Polly.Retry;
using Serilog;

namespace Billing.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingServices(this IServiceCollection services)
    {
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        services.AddSingleton<IPaymentGateway, StripeMockGateway>();
        services.AddSingleton<IPaymentGateway, PayPalMockGateway>();
        services.AddSingleton<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSingleton<IOrderValidator, OrderValidator>();

        services.AddSingleton<ResiliencePipeline<PaymentResult>>(_ =>
        {
            var retryOptions = new RetryStrategyOptions<PaymentResult>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<PaymentResult>()
                    .HandleResult(result => !result.Success && result.IsTransient),
                OnRetry = args =>
                {
                    Log.Warning(
                        "Retrying payment attempt {Attempt} after transient failure",
                        args.AttemptNumber + 1);
                    return ValueTask.CompletedTask;
                }
            };

            return new ResiliencePipelineBuilder<PaymentResult>()
                .AddRetry(retryOptions)
                .Build();
        });

        return services;
    }
}
