using AshproSAASERP.Interface.Services;
using AshproSAASERP.Repo.Services.Storage;

namespace AshproSAASERP.Repo.Services
{
    public class CommonImageRepo : ICommonImageRepo
    {
        public CommonImageRepo()
        {
        }

        /// <summary>
        /// Gets storage service based on provided parameters.
        /// IMPORTANT: Backend is multi-tenant, so we CANNOT use PermanentSettingsData static class.
        /// All configuration must be passed from frontend via parameters.
        /// </summary>
        private IImageStorageService GetStorageService(
            string basePath,
            string clientFolder,
            string? mode = null,
            string? region = null,
            string? bucket = null,
            string? accessKey = null,
            string? secretKey = null)
        {
            // Use provided mode, default to "0" (Local) if not specified
            var effectiveMode = mode ?? "0";

            if (effectiveMode == "1") // AWS S3
            {
                // For AWS S3, all credentials are REQUIRED from frontend
                if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(bucket) ||
                    string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("AWS S3 storage requires region, bucket, accessKey, and secretKey parameters from frontend.");
                }

                return new AwsImageStorageService(region, bucket, accessKey, secretKey, clientFolder);
            }
            else // Local (Default)
            {
                // For local storage, use the provided basePath
                return new LocalImageStorageService(basePath, clientFolder);
            }
        }

        public async Task<string> UploadImageAsync(
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
            string? secretKey = null)
        {
            // Pass mode and AWS parameters to storage service
            var service = GetStorageService(basePath, clientFolder, mode, region, bucket, accessKey, secretKey);
            return await service.SaveImageAsync(stream, fileName, entityType, entityId);
        }

        public async Task<Stream?> GetImageStreamAsync(string basePath, string relativePath, string? mode = null, string? region = null, string? bucket = null, string? accessKey = null, string? secretKey = null)
        {
            var service = GetStorageService(basePath, "", mode, region, bucket, accessKey, secretKey);
            return await service.GetImageStreamAsync(relativePath);
        }

        public async Task<string?> GetImagePathAsync(string basePath, string relativePath, string? mode = null, string? region = null, string? bucket = null, string? accessKey = null, string? secretKey = null)
        {
            var service = GetStorageService(basePath, "", mode, region, bucket, accessKey, secretKey);
            return await service.GetImageUrlAsync(relativePath);
        }

        public async Task<bool> DeleteImageAsync(
            string basePath,
            string relativePath,
            string clientFolder,
            string? mode = null,
            string? region = null,
            string? bucket = null,
            string? accessKey = null,
            string? secretKey = null)
        {
            // Pass mode and AWS parameters to storage service
            var service = GetStorageService(basePath, clientFolder, mode, region, bucket, accessKey, secretKey);
            return await service.DeleteImageAsync(relativePath);
        }
    }
}
