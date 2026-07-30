using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Acme.LegalTech.Contracts;

public interface IContractDocumentAppService : IApplicationService
{
    Task<ContractDocumentVersionDto> GetAsync(Guid id);
    Task<ContractDocumentVersionDto> UploadAsync(Guid contractId, ContractAttachDocumentDto input);
    Task<ListResultDto<ContractDocumentVersionDto>> GetVersionsAsync(Guid contractId);
    Task<IRemoteStreamContent> DownloadAsync(Guid versionId);
    Task DeleteVersionAsync(Guid versionId);
}
