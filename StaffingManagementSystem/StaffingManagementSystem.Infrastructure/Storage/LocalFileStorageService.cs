using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.Interfaces;

namespace StaffingManagementSystem.Infrastructure.Storage
{
    /// <summary>
    /// Stores candidate attachments on local disk on the API server, under a folder per
    /// candidate. This is the Phase 1 storage approach agreed for the RMS project; a future
    /// phase could swap this implementation for cloud blob storage without touching callers.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _rootPath;

        public LocalFileStorageService(IOptions<FileStorageSettings> options)
        {
            var configuredRoot = options.Value.RootPath;

            _rootPath = Path.IsPathRooted(configuredRoot)
                ? configuredRoot
                : Path.Combine(AppContext.BaseDirectory, configuredRoot);

            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> SaveAsync(Guid candidateId, string originalFileName, Stream content, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativePath = Path.Combine(candidateId.ToString("N"), storedFileName);
            var fullPath = Path.Combine(_rootPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(fileStream, cancellationToken);

            // Normalize to forward slashes so the stored value is portable across OS boundaries.
            return relativePath.Replace('\\', '/');
        }

        public Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken = default)
        {
            var fullPath = ResolveFullPath(storedPath);

            if (!File.Exists(fullPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }

        public void Delete(string storedPath)
        {
            var fullPath = ResolveFullPath(storedPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private string ResolveFullPath(string storedPath)
            => Path.Combine(_rootPath, storedPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
