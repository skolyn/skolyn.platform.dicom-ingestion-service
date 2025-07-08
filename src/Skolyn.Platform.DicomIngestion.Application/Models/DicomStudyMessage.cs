namespace Skolyn.Platform.DicomIngestion.Application.Models
{
    public class DicomStudyMessage
    {
        public Guid TraceId { get; set; }
        public string StudyInstanceUid { get; set; }
        public string StorageLocation { get; set; }
        public DateTime IngestionTimestamp { get; set; }
    }
}