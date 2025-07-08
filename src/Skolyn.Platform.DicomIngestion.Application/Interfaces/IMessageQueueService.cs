using Skolyn.Platform.DicomIngestion.Application.Models;

namespace Skolyn.Platform.DicomIngestion.Application.Interfaces
{
    public interface IMessageQueueService
    {
        Task PublishStudyForProcessingAsync(DicomStudyMessage message, CancellationToken cancellationToken);
     
    }
}