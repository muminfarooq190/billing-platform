using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Repositories;
using GeoLeadsService.Infrastructure.Persistence;
using GeoLeadsService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=geo_leads;Username=billing_user;Password=changeme";

        builder.Services.AddDbContext<GeoLeadsDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddScoped<IGeoAreaQueryRepository, GeoAreaQueryRepository>();
        builder.Services.AddScoped<ILeadSourceRecordRepository, LeadSourceRecordRepository>();
        builder.Services.AddScoped<IGeoLeadCatalog, SeededGeoLeadCatalog>();
        builder.Services.AddScoped<IGeoLeadSourceAdapter, SeededGeoLeadSourceAdapter>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddHealthChecks();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GeoLeadsDbContext>();
            db.Database.EnsureCreated();
        }
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
