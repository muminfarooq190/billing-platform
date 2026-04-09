namespace ApiGateway.Configuration;

public sealed class FeatureEntitlementOptions
{
    public List<FeatureRoutePolicy> Routes { get; init; } = [];
}

public sealed class FeatureRoutePolicy
{
    public string Method { get; init; } = string.Empty;
    public string PathPrefix { get; init; } = string.Empty;
    public string FeatureKey { get; init; } = string.Empty;
}
