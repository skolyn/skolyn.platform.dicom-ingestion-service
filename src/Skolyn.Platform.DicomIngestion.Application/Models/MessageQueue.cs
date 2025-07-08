namespace Skolyn.Platform.DicomIngestion.Application.Models
{
    public class MessageQueueSettings
    {
        public string HostName { get; set; } = string.Empty;
        public string ExchangeName { get; set; } = string.Empty;
    }
}
