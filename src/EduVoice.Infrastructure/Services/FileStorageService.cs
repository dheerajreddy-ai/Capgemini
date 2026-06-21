using Amazon.S3;
using Amazon.S3.Model;
using EduVoice.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _bucketName;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _bucketName = configuration["AWS_S3_BUCKET"] ?? "eduvoice-logos";
    }

    private IAmazonS3 CreateS3Client()
    {
        var accessKey = _configuration["AWS_ACCESS_KEY_ID"];
        var secretKey = _configuration["AWS_SECRET_ACCESS_KEY"];
        var region = _configuration["AWS_REGION"] ?? "ap-south-1";

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("AWS credentials not configured");

        return new AmazonS3Client(
            accessKey,
            secretKey,
            Amazon.RegionEndpoint.GetBySystemName(region)
        );
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            using var s3Client = CreateS3Client();

            var key = $"uploads/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await s3Client.PutObjectAsync(request);

            var region = _configuration["AWS_REGION"] ?? "ap-south-1";
            return $"https://{_bucketName}.s3.{region}.amazonaws.com/{key}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            using var s3Client = CreateS3Client();

            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            await s3Client.DeleteObjectAsync(_bucketName, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileUrl}", fileUrl);
        }
    }

    public async Task<string> GetPresignedUrlAsync(string fileName, int expiryMinutes = 60)
    {
        try
        {
            using var s3Client = CreateS3Client();

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };

            return s3Client.GetPreSignedURL(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get presigned URL for {FileName}", fileName);
            throw;
        }
    }
}
