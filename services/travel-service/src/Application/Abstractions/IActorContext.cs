namespace TravelService.Application.Abstractions;

public interface IActorContext
{
    Guid? UserId { get; }
    Guid TenantId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
