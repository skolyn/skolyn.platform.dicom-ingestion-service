using Microsoft.AspNetCore.Mvc;
using Skolyn.Platform.DicomIngestion.Application.Models;
using Skolyn.Platform.DicomIngestion.Application.Services;
using System.Net;

namespace Skolyn.Platform.DicomIngestion.Api.Controllers;

[ApiController]
[Route("api/dicom")]
public class DicomController : ControllerBase
{
    private readonly ILogger<DicomController> _logger;
    private readonly DicomIngestionService _ingestionService;

    public DicomController(ILogger<DicomController> logger, DicomIngestionService ingestionService)
    {
        _logger = logger;
        _ingestionService = ingestionService;
    }

    // Implements the DICOMweb STOW-RS standard
    [HttpPost("studies/{studyInstanceUid}")]
    [Consumes("application/dicom")]
    [ProducesResponseType(typeof(IngestionResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> StoreInstance(string studyInstanceUid)
    {
        _logger.LogInformation("Received DICOM ingestion request for Study UID: {StudyUID}", studyInstanceUid);

        if (Request.Body == null)
        {
            _logger.LogWarning("Request body is null for Study UID: {StudyUID}", studyInstanceUid);
            return BadRequest("Request body cannot be null.");
        }

        try
        {
            using var stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            stream.Position = 0;

            var response = await _ingestionService.IngestStudyAsync(stream, studyInstanceUid, HttpContext.RequestAborted);

            _logger.LogInformation("Successfully ingested Study UID: {StudyUID}. Trace ID: {TraceId}", studyInstanceUid, response.TraceId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during ingestion for Study UID: {StudyUID}", studyInstanceUid);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
        }
    }
}
