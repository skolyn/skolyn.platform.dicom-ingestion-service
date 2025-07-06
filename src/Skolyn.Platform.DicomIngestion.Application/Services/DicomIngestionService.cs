using Skolyn.Platform.DicomIngestion.Application.Interfaces;
using Skolyn.Platform.DicomIngestion.Application.Models;
using Microsoft.Extensions.Logging;

namespace Skolyn.Platform.DicomIngestion.Application.Services;

public class DicomIngestionService
{
    private readonly IObjectStorageService _storageService;
    private readonly IMessageQueueService _messageQueueService;
    private readonly ILogger<DicomIngestionService> _logger;

    public DicomIngestionService(IObjectStorageService storageService, IMessageQueueService messageQueueService, ILogger<DicomIngestionService> logger)
    {
        _storageService = storageService;
        _messageQueueService = messageQueueService;
        _logger = logger;
    }

    public async Task<IngestionResponse> IngestStudyAsync(Stream dicomStream, string studyInstanceUid, CancellationToken cancellationToken)
    {
        var traceId = Guid.NewGuid();
        _logger.LogInformation("[Trace ID: {TraceId}] Starting ingestion process.", traceId);

        // Step 1: Upload to durable object storage
        var objectKey = $"incoming/{DateTime.UtcNow:yyyy-MM-dd}/{studyInstanceUid}_{traceId}.dcm";
        var storageUrl = await _storageService.UploadFileAsync(objectKey, dicomStream, cancellationToken);
        _logger.LogInformation("[Trace ID: {TraceId}] File successfully uploaded to: {StorageUrl}", traceId, storageUrl);

        // Step 2: Create a message for the processing pipeline
        var message = new DicomStudyMessage
        {
            TraceId = traceId,
            StudyInstanceUid = studyInstanceUid,
            StorageLocation = storageUrl,
            IngestionTimestamp = DateTime.UtcNow
        };

        // Step 3: Publish the message to the queue
        await _messageQueueService.PublishStudyForProcessingAsync(message, cancellationToken);
        _logger.LogInformation("[Trace ID: {TraceId}] Message successfully published to queue.", traceId);

        // Step 4: Return success response
        return new IngestionResponse
        {
            Status = "Success",
            TraceId = traceId,
            Message = "DICOM study has been successfully queued for processing."
        };
    }
}
