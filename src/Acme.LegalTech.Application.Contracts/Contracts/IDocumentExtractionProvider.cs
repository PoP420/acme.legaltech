using System.Threading;
using System.Threading.Tasks;
using Acme.LegalTech.Common;
using Volo.Abp.Content;

namespace Acme.LegalTech.Contracts;

public interface IDocumentExtractionProvider
{
    Task<DocumentExtractionResult> ExtractAsync(
        IRemoteStreamContent document,
        string contentType,
        CancellationToken cancellationToken = default);
}
