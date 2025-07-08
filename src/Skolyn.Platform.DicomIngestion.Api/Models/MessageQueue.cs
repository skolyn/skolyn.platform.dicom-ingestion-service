namespace Skolyn.Platform.DicomIngestion.Api.Models
{
    public class MessageQueueSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string ExchangeName { get; set; } = string.Empty;
    }
}
