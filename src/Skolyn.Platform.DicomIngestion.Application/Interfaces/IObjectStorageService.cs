using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skolyn.Platform.DicomIngestion.Application.Interfaces
{
    public interface IObjectStorageService
    {
        Task<string> UploadFileAsync(
        string objectKey,
        Stream fileStream,
        CancellationToken cancellationToken);
    }
}
