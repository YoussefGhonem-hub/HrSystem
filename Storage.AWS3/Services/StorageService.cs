using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Storage.AWS3.Extensions;
using Storage.AWS3.Models;
using System.Net;

namespace Storage.AWS3.Services
{
    public class StorageService : IStorageService
    {
        private readonly AWS3Options _storageConfig;
        private readonly string _bucketName;
        private readonly IConfiguration _configuration;
        public StorageService(IConfiguration configuration)
        {
            _storageConfig = configuration.GetAWSConfigurationOptions();
            _bucketName = _storageConfig.DefaultBucket;
            _configuration = configuration;
        }

        public async Task<StoredFile> Upload(IFormFile? file, CancellationToken cancellationToken = default)
        {
            if (file is null)
                return new StoredFile();

            try
            {
                // Load AWS config
                var options = AWS3OptionsExtension.GetAWSConfigurationOptions(_configuration);
                var region = RegionEndpoint.EUNorth1;
                var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(_configuration);

                // Validate credentials
                if (credential == null)
                    throw new ArgumentException("AWS credentials not found.");

                var key = Guid.NewGuid();
                var stream = file.OpenReadStream();
                var bucketName = options.DefaultBucket;
                var fileNameStorage = GetFileName(key, file.FileName);

                var uploadRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileNameStorage,
                    InputStream = stream,
                    ContentType = GetContentType(file.FileName)
                };

                uploadRequest.Metadata.Add("Content-Type", uploadRequest.ContentType);

                var client = new AmazonS3Client(credential, region);
                await EnsureBucketExistsAsync(client, bucketName, region, cancellationToken);

                // Upload to S3
                await client.PutObjectAsync(uploadRequest, cancellationToken);

                return new StoredFile
                {
                    FileName = file.FileName,
                    Key = fileNameStorage,
                    Extension = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                    Url = GetUploadedFileUrl(fileNameStorage)

                };
            }
            catch (AmazonS3Exception awsEx)
            {
                // Log AWS-specific errors
                Console.WriteLine($"AWS S3 Error: {awsEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload Error: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadLocalVideoToS3Async(string localFilePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("File not found", localFilePath);
            // Load AWS config
            var options = AWS3OptionsExtension.GetAWSConfigurationOptions(_configuration);
            var region = RegionEndpoint.EUNorth1;
            var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(_configuration);

            // Validate credentials
            if (credential == null)
                throw new ArgumentException("AWS credentials not found.");

            var client = new AmazonS3Client(credential, region);

            var fileTransferUtility = new TransferUtility(client);
            var key = Guid.NewGuid();
            var bucketName = options.DefaultBucket;
            var safeTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var fileNameStorage = GetFileName(key, $"MergeVideo_{safeTimestamp}.mp4");
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                FilePath = localFilePath,
                Key = fileNameStorage,
                ContentType = "video/mp4", // Adjust if needed
            };

            await fileTransferUtility.UploadAsync(uploadRequest);

           return GetUploadedFileUrl(fileNameStorage);
        }
        public async Task<List<StoredFile>?> UploadFiles(List<IFormFile>? files, CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0)
            {
                return new List<StoredFile>();
            }

            var uploadedFiles = new List<StoredFile>();

            foreach (var file in files)
            {
                var result = await Upload(file, cancellationToken);
                uploadedFiles.Add(result);
            }

            return uploadedFiles;
        }
        public async Task<bool> Delete(string key, CancellationToken cancellationToken = default)
        {
            var options = AWS3OptionsExtension.GetAWSConfigurationOptions(_configuration);
            var region = RegionEndpoint.EUNorth1;
            var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(_configuration);
            var bucketName = options.DefaultBucket;

            var client = new AmazonS3Client(credential, region);
            await EnsureBucketExistsAsync(client, bucketName, region, cancellationToken);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            try
            {
                var response = await client.DeleteObjectAsync(deleteRequest, cancellationToken);
                // Check the response for success
                return response.HttpStatusCode == HttpStatusCode.NoContent;
            }
            catch (AmazonS3Exception ex)
            {
                // Handle exceptions, such as object not found
                // Log the error or perform other error handling as needed
                return false;
            }
        }
        public async Task<string?> DownloadFileUrl(string? key, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return null;

                var normalizedKey = NormalizeKeyOrUrl(key);
                if (string.IsNullOrWhiteSpace(normalizedKey))
                {
                    return key;
                }

                var options = AWS3OptionsExtension.GetAWSConfigurationOptions(_configuration);
                var region = RegionEndpoint.EUNorth1;
                var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(_configuration);
                var bucketName = options.DefaultBucket;

                var client = new AmazonS3Client(credential, region);
                await EnsureBucketExistsAsync(client, bucketName, region, cancellationToken);

                // Generate a pre-signed URL for the object with a specific key
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = normalizedKey,
                    Expires = DateTime.Now.AddHours(1) // Adjust expiration as needed

                };

                var url = client.GetPreSignedURL(request);
                return url;
            }
            catch (AmazonS3Exception e)
            {
                throw;

            }
            catch (Exception)
            {

                throw;
            }

        }

        private string? NormalizeKeyOrUrl(string value)
        {
            var raw = value.Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            // Already a storage key.
            if (!raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return raw.TrimStart('/');
            }

            if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            {
                return raw;
            }

            var host = uri.Host;
            var looksLikeS3Host = host.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase) ||
                                  host.Contains("cloudfront.net", StringComparison.OrdinalIgnoreCase) ||
                                  host.StartsWith(_bucketName + ".", StringComparison.OrdinalIgnoreCase);

            // Non-S3 URL should be returned untouched.
            if (!looksLikeS3Host)
            {
                return null;
            }

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (segments.Count == 0)
            {
                return null;
            }

            // Path-style URL: /{bucket}/{key}
            if (segments.Count > 1 &&
                string.Equals(segments[0], _bucketName, StringComparison.OrdinalIgnoreCase))
            {
                segments.RemoveAt(0);
            }

            return string.Join('/', segments);
        }
        public async Task<DownloadedFile> DownloadFile(string key, CancellationToken cancellationToken)
        {
            try
            {
                var options = AWS3OptionsExtension.GetAWSConfigurationOptions(_configuration);
                var bucketName = options.DefaultBucket;
                var region = RegionEndpoint.EUNorth1;
                var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(_configuration);
                
                using (var s3Client = new AmazonS3Client(credential, region))
                {
                    // Download directly to memory without saving to disk
                    var request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };

                    using (var response = await s3Client.GetObjectAsync(request, cancellationToken))
                    using (var memoryStream = new MemoryStream())
                    {
                        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                        
                        // Determine the content type
                        string contentType = !string.IsNullOrEmpty(response.Headers.ContentType) 
                            ? response.Headers.ContentType 
                            : GetContentType(key);

                        // Get filename from key
                        var fileName = Path.GetFileName(key);

                        return new DownloadedFile(memoryStream.ToArray(), contentType, fileName);
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"S3 Error downloading file '{key}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file '{key}': {ex.Message}", ex);
            }
        }
        public async Task<string> DownloadVideoFromS3ToLocalAsync(string keyOrUrl, string localFileName, CancellationToken cancellationToken)
        {
            var videoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
            if (!Directory.Exists(videoDirectory))
                Directory.CreateDirectory(videoDirectory);

            var localPath = Path.Combine(videoDirectory, localFileName);

            // If it's a full URL, download directly
            if (keyOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(keyOrUrl);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);

                return localPath;
            }

            // Otherwise assume it's a key — get presigned URL and download it
            var url = await DownloadFileUrl(keyOrUrl, cancellationToken);
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Failed to get presigned S3 URL.");

            using (var client = new HttpClient())
            {
                using var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);
            }

            return localPath;
        }

        public string GetUploadedFileUrl(string key)
        {
            var region = _storageConfig.Region;
            var bucketName = _bucketName;

            // If the bucket is public, construct the direct URL
            return $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
        }
        #region Helpers
        private static string GetFileName(Guid blobId, string contentType)
        {
            var fileSplit = contentType.Split('.');
            var fileExtension = fileSplit[^1];
            var blobFileName = blobId.ToString();
            if (fileSplit.Length > 1) blobFileName += "." + fileExtension;
            return blobFileName;
        }

        private static string GetContentType(string? filePath)
        {

            // Determine content type based on file extension
            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".pdf":
                    return "application/pdf";
                // Add more cases for other file types as needed
                default:
                    return "application/octet-stream"; // Default content type for unknown types
            }
        }
        private async Task EnsureBucketExistsAsync(IAmazonS3 client, string bucketName, RegionEndpoint region, CancellationToken cancellationToken = default)
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
            if (!exists)
            {
                Console.WriteLine($"Bucket '{bucketName}' does not exist. Creating...");

                var createRequest = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                var response = await client.PutBucketAsync(createRequest, cancellationToken);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AmazonS3Exception($"Failed to create bucket '{bucketName}'. Status: {response.HttpStatusCode}");
                }
            }
        }
        #endregion
    }
}
