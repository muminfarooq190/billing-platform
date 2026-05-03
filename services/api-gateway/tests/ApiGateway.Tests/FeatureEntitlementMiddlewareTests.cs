using ApiGateway.Middleware;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Tests;

public sealed class FeatureEntitlementMiddlewareTests
{
    [Theory]
    [InlineData("POST", "/api/travel/quotations", "travel.quotation.create")]
    [InlineData("POST", "/api/travel/bookings/from-quotation/123", "travel.booking.create")]
    [InlineData("GET", "/api/travel/timeline/Quotation/123", "travel.timeline.read")]
    [InlineData("POST", "/api/communication/notifications", "communication.notification.send")]
    [InlineData("GET", "/api/communication/notifications/recipient/11111111-1111-1111-1111-111111111111", "communication.logs.read")]
    [InlineData("PUT", "/api/communication/templates/123", "communication.templates.manage")]
    [InlineData("GET", "/api/identity/audit/export", "identity.audit.export")]
    [InlineData("PUT", "/api/identity/users/11111111-1111-1111-1111-111111111111/roles", "identity.rbac.advanced")]
    [InlineData("GET", "/api/geo-leads/queries", "geo-leads.read")]
    [InlineData("POST", "/api/geo-leads/saved-areas", "geo-leads.manage")]
    public void MatchFeature_ShouldResolveMappedPremiumRoutes(string method, string path, string expected)
    {
        var routes = new[]
        {
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "/api/travel/quotations", FeatureKey = "travel.quotation.create" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "/api/travel/bookings/from-quotation", FeatureKey = "travel.booking.create" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "GET", PathPrefix = "/api/travel/timeline", FeatureKey = "travel.timeline.read" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "/api/communication/notifications", FeatureKey = "communication.notification.send" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "GET", PathPrefix = "/api/communication/notifications/recipient", FeatureKey = "communication.logs.read" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "PUT", PathPrefix = "/api/communication/templates", FeatureKey = "communication.templates.manage" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "GET", PathPrefix = "/api/identity/audit/export", FeatureKey = "identity.audit.export" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "PUT", PathPrefix = "/api/identity/users", FeatureKey = "identity.rbac.advanced" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "GET", PathPrefix = "/api/geo-leads", FeatureKey = "geo-leads.read" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "/api/geo-leads", FeatureKey = "geo-leads.manage" }
        };
        var result = FeatureEntitlementMiddleware.MatchFeature(routes, method, new PathString(path));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("GET", "/health")]
    [InlineData("GET", "/api/auth/login")]
    [InlineData("GET", "/api/travel/public/quote/token")]
    [InlineData("GET", "/api/identity/users/me")]
    public void MatchFeature_ShouldReturnNull_ForUnmappedRoutes(string method, string path)
    {
        var routes = new[]
        {
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "/api/travel/quotations", FeatureKey = "travel.quotation.create" }
        };
        var result = FeatureEntitlementMiddleware.MatchFeature(routes, method, new PathString(path));
        Assert.Null(result);
    }

    [Fact]
    public void MatchFeature_ShouldPreferPathPattern_WhenConfigured()
    {
        var routes = new[]
        {
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPattern = "/api/travel/quotations/{id}/send", FeatureKey = "travel.quotation.send" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPattern = "/api/travel/quotations/{id}/revisions", FeatureKey = "travel.quotation.write" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPattern = "/api/travel/bookings/{id}/documents", FeatureKey = "travel.booking.documents.upload" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPattern = "/api/travel/inquiries/{id}/assign", FeatureKey = "travel.inquiries.write" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "PATCH", PathPattern = "/api/travel/bookings/{id}/items/{itemId}/status", FeatureKey = "travel.bookings.write" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "PUT", PathPattern = "/api/travel/itineraries/{id}", FeatureKey = "travel.itineraries.write" }
        };

        var sendResult = FeatureEntitlementMiddleware.MatchFeature(routes, "POST", new PathString("/api/travel/quotations/123/send"));
        var revisionResult = FeatureEntitlementMiddleware.MatchFeature(routes, "POST", new PathString("/api/travel/quotations/123/revisions"));
        var docsResult = FeatureEntitlementMiddleware.MatchFeature(routes, "POST", new PathString("/api/travel/bookings/123/documents"));
        var assignResult = FeatureEntitlementMiddleware.MatchFeature(routes, "POST", new PathString("/api/travel/inquiries/123/assign"));
        var itemStatusResult = FeatureEntitlementMiddleware.MatchFeature(routes, "PATCH", new PathString("/api/travel/bookings/123/items/456/status"));
        var itineraryUpdateResult = FeatureEntitlementMiddleware.MatchFeature(routes, "PUT", new PathString("/api/travel/itineraries/123"));

        Assert.Equal("travel.quotation.send", sendResult);
        Assert.Equal("travel.quotation.write", revisionResult);
        Assert.Equal("travel.booking.documents.upload", docsResult);
        Assert.Equal("travel.inquiries.write", assignResult);
        Assert.Equal("travel.bookings.write", itemStatusResult);
        Assert.Equal("travel.itineraries.write", itineraryUpdateResult);
    }

    [Fact]
    public void MatchFeature_ShouldIgnoreBrokenConfigEntries()
    {
        var routes = new[]
        {
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "", FeatureKey = "travel.quotation.create" },
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "POST", PathPrefix = "/api/travel/quotations", FeatureKey = "travel.quotation.create" }
        };

        var result = FeatureEntitlementMiddleware.MatchFeature(routes, "POST", new PathString("/api/travel/quotations/123"));
        Assert.Equal("travel.quotation.create", result);
    }
}
