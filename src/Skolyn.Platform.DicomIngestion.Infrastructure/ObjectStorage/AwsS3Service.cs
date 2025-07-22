using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Skolyn.Platform.DicomIngestion.Application.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class AwsS3Service : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _serviceUrl;

    public AwsS3Service(IOptions<ObjectStorageSettings> options)
    {
        var config = options.Value ?? throw new ArgumentNullException(nameof(options));

        _bucketName = config.BucketName ?? throw new ArgumentNullException("ObjectStorage:BucketName is required.");
        _serviceUrl = config.ServiceUrl;

        var region = RegionEndpoint.GetBySystemName(config.Region ?? "us-east-1");

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = region,
            ForcePathStyle = true
        };

        if (!string.IsNullOrEmpty(config.ServiceUrl))
        {
            s3Config.ServiceURL = config.ServiceUrl;
        }

        _s3Client = new AmazonS3Client(config.AccessKey ?? "test", config.SecretKey ?? "test", s3Config);
    }

    public async Task<string> UploadFileAsync(string objectKey, Stream fileStream, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = fileStream,
            CannedACL = S3CannedACL.PublicRead
        };

        var response = await _s3Client.PutObjectAsync(request, cancellationToken);

        var baseUrl = _serviceUrl ?? $"https://{_bucketName}.s3.amazonaws.com";
        var url = $"{baseUrl}/{_bucketName}/{objectKey}";

        return url;
    }
}
