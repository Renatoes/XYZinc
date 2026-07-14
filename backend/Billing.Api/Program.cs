using Billing.Api.Endpoints;
using Billing.Api.Extensions;
using Billing.Api.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddBillingServices();

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(
                    builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                    ?? ["http://localhost:3000"])
                .AllowAnyHeader()
                .AllowAnyMethod()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "XYZ Inc. Billing API",
            Version = "v1",
            Description = "Submit orders and process payments through mocked gateways."
        });
    });

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing API v1");
        });
    }

    app.UseCors();
    app.UseHttpsRedirection();
    app.MapOrderEndpoints();

    Log.Information("Billing API starting on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Billing API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
