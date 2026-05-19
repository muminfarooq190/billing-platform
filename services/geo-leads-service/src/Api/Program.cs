using GeoLeadsService.Api.Filters;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Repositories;
using GeoLeadsService.Infrastructure.Entitlements;
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

        builder.Services.AddDbContext<GeoLeadsDbContext>(options => options.UseNpgsql(databaseUrl, x => x.UseNetTopologySuite()));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddScoped<IGeoAreaQueryRepository, GeoAreaQueryRepository>();
        builder.Services.AddScoped<ILeadSourceRecordRepository, LeadSourceRecordRepository>();
        builder.Services.AddScoped<ILeadSourceIngestionRunRepository, LeadSourceIngestionRunRepository>();
        builder.Services.AddScoped<ISavedGeoAreaRepository, SavedGeoAreaRepository>();
        builder.Configuration.AddJsonFile("Api/appsettings.json", optional: true, reloadOnChange: false);

        builder.Services.AddScoped<IGeoLeadCatalog, PostGisGeoLeadCatalog>();
        builder.Services.AddScoped<IGeoLeadSourceAdapter, SeededGeoLeadSourceAdapter>();
        builder.Services.AddScoped<IGeoLeadSourceAdapter, PublicDirectoryGeoLeadSourceAdapter>();
        builder.Services.AddScoped<IGeoLeadSourceAdapter, OverpassGeoLeadSourceAdapter>();
        // Overpass HTTP client. Endpoint can be overridden via
        // `GeoLeadSources:Overpass:Endpoint` (e.g. self-hosted overpass instance).
        builder.Services.AddHttpClient("overpass", c => c.Timeout = TimeSpan.FromSeconds(60));
        builder.Services.AddHttpClient<IBillingEntitlementsClient, BillingEntitlementsClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://billing-service:8080/");
        });
        builder.Services.AddScoped<IFeatureGate, CachedFeatureGate>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddHealthChecks();
        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GeoLeadsDbContext>();
            db.Database.Migrate();
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
