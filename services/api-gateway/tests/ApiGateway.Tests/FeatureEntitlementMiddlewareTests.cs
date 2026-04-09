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
    [InlineData("PUT", "/api/communication/templates/123", "communication.templates.manage")]
    public void MatchFeature_ShouldResolveMappedPremiumRoutes(string method, string path, string expected)
    {
        var result = FeatureEntitlementMiddleware.MatchFeature(method, new PathString(path));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("GET", "/health")]
    [InlineData("GET", "/api/auth/login")]
    [InlineData("GET", "/api/travel/public/quote/token")]
    public void MatchFeature_ShouldReturnNull_ForUnmappedRoutes(string method, string path)
    {
        var result = FeatureEntitlementMiddleware.MatchFeature(method, new PathString(path));
        Assert.Null(result);
    }
}
