namespace Skolyn.Platform.DicomIngestion.Application.Models
{
    public class IngestionResponse
    {
        public string Status { get; set; }
        public Guid TraceId { get; set; }
        public string Message { get; set; }
    }
}