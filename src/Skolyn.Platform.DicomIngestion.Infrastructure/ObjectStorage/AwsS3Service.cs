using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Skolyn.Platform.DicomIngestion.Application.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class AwsS3Service : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public AwsS3Service(IConfiguration config)
    {
        _bucketName = config["ObjectStorage:BucketName"]
                      ?? throw new ArgumentNullException("ObjectStorage:BucketName is required.");

        var accessKey = config["ObjectStorage:AccessKey"] ?? "test";
        var secretKey = config["ObjectStorage:SecretKey"] ?? "test";
        var regionName = config["ObjectStorage:Region"] ?? "us-east-1";
        var serviceUrl = config["ObjectStorage:ServiceUrl"];
        var region = RegionEndpoint.GetBySystemName(regionName);

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = region,
            ForcePathStyle = true
        };

        if (!string.IsNullOrEmpty(serviceUrl))
        {
            s3Config.ServiceURL = serviceUrl;
        }

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);

      
    }
   


    public async Task<string> UploadFileAsync(string objectKey, Stream fileStream, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = fileStream,
            CannedACL = S3CannedACL.PublicRead // linklə baxmaq istəyirsənsə, bu vacibdir!
        };

        var response = await _s3Client.PutObjectAsync(request, cancellationToken);

        // LocalStack üçün uyğun URL qaytar
        var baseUrl = _s3Client.Config.ServiceURL ?? $"https://{_bucketName}.s3.amazonaws.com";
        var url = $"{baseUrl}/{_bucketName}/{objectKey}";

        return url;
    }

}
