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
            new ApiGateway.Configuration.FeatureRoutePolicy { Method = "PUT", PathPrefix = "/api/identity/users", FeatureKey = "identity.rbac.advanced" }
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
