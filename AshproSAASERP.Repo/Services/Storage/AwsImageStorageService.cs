using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AshproSAASERP.Interface.Services;

namespace AshproSAASERP.Repo.Services.Storage;

/// <summary>
/// AWS S3 image storage implementation for backend
/// Stores images in: s3://{Bucket}/{ClientFolder}/{EntityType}/{EntityId}/
/// </summary>
public class AwsImageStorageService : IImageStorageService
{
    private readonly string _region;
    private readonly string _bucket;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _clientFolder;
    private readonly AmazonS3Client _s3Client;

    public AwsImageStorageService(string region, string bucket, string accessKey, string secretKey, string clientFolder)
    {
        _region = region ?? throw new ArgumentNullException(nameof(region));
        _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        _accessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey));
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _clientFolder = clientFolder ?? throw new ArgumentNullException(nameof(clientFolder));

        // Create AWS credentials and S3 client
        var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region)
        };
        _s3Client = new AmazonS3Client(credentials, config);
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

            // Build S3 key: ClientFolder/EntityType/EntityId/filename
            var s3Key = $"{_clientFolder}/{entityType}/{entityId}/{fileName}";

            // Reset stream position if seekable
            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            // Upload to S3
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = s3Key,
                InputStream = imageStream,
                ContentType = GetContentType(extension),
                CannedACL = S3CannedACL.Private // Private - only accessible via presigned URLs
            };

            var response = await _s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to upload to S3. Status: {response.HttpStatusCode}");
            }

            return s3Key;
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"AWS S3 error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save image to S3: {ex.Message}", ex);
        }
    }

    public async Task<string> GetImageUrlAsync(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            // Generate presigned URL valid for 1 hour
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = relativePath,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            var url = _s3Client.GetPreSignedURL(request);
            return url;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate presigned URL: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = relativePath
            };

            var response = await _s3Client.DeleteObjectAsync(deleteRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception($"Failed to delete from S3. Status: {response.HttpStatusCode}");
            }

            return true;
        }
        catch (AmazonS3Exception ex)
        {
            // If object doesn't exist, consider it already deleted
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            throw new Exception($"AWS S3 error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete image from S3: {ex.Message}", ex);
        }
    }

    public async Task<bool> ImageExistsAsync(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucket,
                Key = relativePath
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to check image existence in S3: {ex.Message}", ex);
        }
    }

    public async Task<Stream?> GetImageStreamAsync(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            var request = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = relativePath
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get image stream from S3: {ex.Message}", ex);
        }
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}



