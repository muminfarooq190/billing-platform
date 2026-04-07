namespace TravelService.Application.Abstractions;

public interface IFileStorage
{
    Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
    Task<string> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken);
    Task<string> GetSignedReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken cancellationToken);
}
