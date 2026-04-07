using TravelService.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace TravelService.Infrastructure.Files;

public sealed class LocalFileStorage(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment) : IFileStorage
{
    private readonly string _rootPath = Path.Combine(environment.ContentRootPath, "storage");

    public async Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken cancellationToken)
    {
        var normalizedPath = path.Replace('\\', '/').TrimStart('/');
        var fullPath = Path.Combine(_rootPath, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath) ?? _rootPath;
        Directory.CreateDirectory(directory);

        await using var targetStream = File.Create(fullPath);
        await stream.CopyToAsync(targetStream, cancellationToken);
        return normalizedPath;
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_rootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<string> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken)
        => Task.FromResult(BuildUrl(storageKey));

    public Task<string> GetSignedReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken cancellationToken)
        => Task.FromResult(BuildUrl(storageKey));

    private string BuildUrl(string storageKey)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return $"/travel/files/{storageKey}";

        return $"{request.Scheme}://{request.Host}/travel/files/{storageKey}";
    }
}
