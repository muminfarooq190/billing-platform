using IdentityService.Application.Abstractions;

namespace IdentityService.Infrastructure.Persistence;

public sealed class LocalBrandAssetStorage(IWebHostEnvironment environment) : IBrandAssetStorage
{
    public async Task SaveAsync(string storageKey, Stream stream, CancellationToken cancellationToken)
    {
        var fullPath = GetAbsolutePath(storageKey);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public string GetAbsolutePath(string storageKey)
    {
        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(environment.ContentRootPath, "storage", normalizedKey);
    }
}
