

namespace ZakiSAASERP.Interface.Services;

/// <summary>
/// Interface for image storage operations supporting multiple storage backends (Local, AWS S3)
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Saves an uploaded image file to configured storage
    /// </summary>
    /// <param name="imageStream">Image content stream</param>
    /// <param name="fileName">Original file name (for extension)</param>
    /// <param name="entityType">Entity type (e.g., "Category", "SubCategory")</param>
    /// <param name="entityId">Entity ID for folder organization</param>
    /// <returns>Relative path to stored image (e.g., "ClientABC/Category/1/logo.png")</returns>
    Task<string> SaveImageAsync(Stream imageStream, string fileName, string entityType, int entityId);

    /// <summary>
    /// Gets the full URL or path to access an image
    /// </summary>
    /// <param name="relativePath">Relative path from database (e.g., "ClientABC/Category/1/logo.png")</param>
    /// <returns>Full URL or path to access the image</returns>
    Task<string> GetImageUrlAsync(string relativePath);

    /// <summary>
    /// Deletes an image from storage
    /// </summary>
    /// <param name="relativePath">Relative path from database</param>
    Task<bool> DeleteImageAsync(string relativePath);

    /// <summary>
    /// Checks if an image exists in storage
    /// </summary>
    /// <param name="relativePath">Relative path from database</param>
    /// <returns>True if image exists</returns>
    Task<bool> ImageExistsAsync(string relativePath);

    /// <summary>
    /// Gets the image as a stream (useful for proxying or server-side processing)
    /// </summary>
    Task<Stream?> GetImageStreamAsync(string relativePath);
}
