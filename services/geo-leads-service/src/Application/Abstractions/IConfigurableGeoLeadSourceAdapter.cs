namespace GeoLeadsService.Application.Abstractions;

public interface IConfigurableGeoLeadSourceAdapter : IGeoLeadSourceAdapter
{
    bool IsEnabled { get; }
}
