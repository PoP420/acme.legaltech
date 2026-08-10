using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Processing;

public interface IIngestionJobAppService : IApplicationService
{
    Task<IngestionJobDto> GetAsync(Guid id);
    Task<ListResultDto<IngestionJobDto>> GetListAsync(Guid contractDocumentVersionId);
    Task<IngestionJobDto> CreateAsync(Guid contractDocumentVersionId, string jobType, string? providerName = null);
    Task<IngestionJobDto> RunAsync(Guid id);
    Task RetryAsync(Guid id);
    Task CancelAsync(Guid id);
}
