using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Repositories;
using GeoLeadsService.Infrastructure.Persistence.Repositories;

namespace GeoLeadsService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddScoped<IGeoAreaQueryRepository, InMemoryGeoAreaQueryRepository>();
        builder.Services.AddScoped<IGeoLeadCatalog, SeededGeoLeadCatalog>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddHealthChecks();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }
}

public interface ITenantContext
{
    Guid TenantId { get; }
}

internal sealed class HeaderTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId
    {
        get
        {
            var header = httpContextAccessor.HttpContext?.Request.Headers["x-tenant-id"].ToString();
            return Guid.TryParse(header, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}
