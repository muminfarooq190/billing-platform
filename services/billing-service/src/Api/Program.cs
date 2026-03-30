namespace BillingService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.MapHealthChecks("/health");
        app.MapGet("/", () => Results.Ok(new { service = "billing-service", status = "ok" }));

        app.Run();
    }
}
