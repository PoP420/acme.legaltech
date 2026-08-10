using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Processing;

public interface IExtractionSuggestionAppService : IApplicationService
{
    Task<ListResultDto<ExtractionSuggestionDto>> GetByJobAsync(Guid ingestionJobId);
    Task<ExtractionSuggestionDto> DecideAsync(Guid id, DecideSuggestionInput input);
}

public interface IRiskAssessmentSuggestionAppService : IApplicationService
{
    Task<ListResultDto<RiskAssessmentSuggestionDto>> GetByJobAsync(Guid ingestionJobId);
    Task<RiskAssessmentSuggestionDto> DecideAsync(Guid id, DecideSuggestionInput input);
}

public class DecideSuggestionInput
{
    public string Decision { get; set; } = string.Empty;
    public string? CorrectedValue { get; set; }
    public string? Comment { get; set; }
}
