namespace AshproSAASERP.Interface.Services
{
    public interface ICommonImageRepo
    {
        /// <summary>
        /// Uploads an image to server storage (Local or AWS S3)
        /// </summary>
        Task<string> UploadImageAsync(
            Stream stream,
            string fileName,
            string entityType,
            int entityId,
            string basePath,
            string clientFolder,
            string? mode = null,
            string? region = null,
            string? bucket = null,
            string? accessKey = null,
            string? secretKey = null);

        /// <summary>
        /// Gets the image file stream for serving via API using relative path
        /// </summary>
        Task<Stream?> GetImageStreamAsync(string basePath, string relativePath, string? mode = null, string? region = null, string? bucket = null, string? accessKey = null, string? secretKey = null);

        /// <summary>
        /// Gets the full physical image path (for Desktop/Native apps) using relative path
        /// </summary>
        Task<string?> GetImagePathAsync(string basePath, string relativePath, string? mode = null, string? region = null, string? bucket = null, string? accessKey = null, string? secretKey = null);

        /// <summary>
        /// Deletes an image file from storage (Local or AWS S3)
        /// </summary>
        Task<bool> DeleteImageAsync(
            string basePath,
            string relativePath,
            string clientFolder,
            string? mode = null,
            string? region = null,
            string? bucket = null,
            string? accessKey = null,
            string? secretKey = null);
    }
}
