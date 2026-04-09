namespace IdentityService.Application.Abstractions;

public interface IBrandAssetStorage
{
    Task SaveAsync(string storageKey, Stream stream, CancellationToken cancellationToken);
    string GetAbsolutePath(string storageKey);
}
