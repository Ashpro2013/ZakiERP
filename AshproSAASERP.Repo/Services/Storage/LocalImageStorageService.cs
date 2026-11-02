using AshproSAASERP.Interface.Services;

namespace AshproSAASERP.Repo.Services.Storage;

/// <summary>
/// Local file system image storage implementation
/// Stores images in: {BasePath}/{ClientFolder}/{EntityType}/{EntityId}/
/// </summary>
public class LocalImageStorageService : IImageStorageService
{
    private readonly string _basePath;
    private readonly string _clientFolder;

    public LocalImageStorageService(string basePath, string clientFolder)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _clientFolder = clientFolder ?? throw new ArgumentNullException(nameof(clientFolder));

        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string entityType, int entityId)
    {
        try
        {
            // Validate input
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (extension is not ".png" and not ".jpg" and not ".jpeg")
                throw new InvalidOperationException("Only PNG, JPG, and JPEG files are allowed");

            // Build relative path: ClientFolder/EntityType/EntityId/filename
            var relativePath = Path.Combine(_clientFolder, entityType, entityId.ToString(), fileName);

            // Build full path for saving
            var fullPath = Path.Combine(_basePath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            // Ensure directory exists
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Reset stream position if seekable
            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            // Save file
            using (var fileStream = File.Create(fullPath))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Verify file was created
            if (!File.Exists(fullPath))
            {
                throw new Exception($"File was not created at: {fullPath}");
            }

            return relativePath;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new Exception($"ACCESS DENIED: No permission to write to {_basePath}\nError: {ex.Message}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new Exception($"DIRECTORY NOT FOUND: {_basePath}\nError: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new Exception($"IO ERROR: Cannot write to {_basePath}\nError: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save image\nBase Path: {_basePath}\nClient Folder: {_clientFolder}\nError: {ex.Message}", ex);
        }
    }

    public Task<string> GetImageUrlAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.FromResult(string.Empty);

        // For local storage, return full local path
        // Note: In web context, you might want to return a web-accessible URL instead
        var fullPath = Path.Combine(_basePath, relativePath);

        // Convert to file:// URL for display (if needed in Blazor)
        // Or return the path directly if using IIS/static file serving
        return Task.FromResult(fullPath);
    }

    public Task<bool> DeleteImageAsync(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.FromResult(false);

            var fullPath = Path.Combine(_basePath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                // Clean up empty directories
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory);
                    }
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete image: {ex.Message}", ex);
        }
    }

    public Task<bool> ImageExistsAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.FromResult(false);

        var fullPath = Path.Combine(_basePath, relativePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<Stream?> GetImageStreamAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.FromResult<Stream?>(null);

        var fullPath = Path.Combine(_basePath, relativePath);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }
}



