using System;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Acme.LegalTech.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Controllers;

[ApiController]
[Route("api/app/ai/suggestions")]
public class SuggestionController : LegalTechController
{
    private readonly IExtractionSuggestionAppService _extractionAppService;
    private readonly IRiskAssessmentSuggestionAppService _riskAppService;

    public SuggestionController(
        IExtractionSuggestionAppService extractionAppService,
        IRiskAssessmentSuggestionAppService riskAppService)
    {
        _extractionAppService = extractionAppService;
        _riskAppService = riskAppService;
    }

    [HttpGet("extraction/{ingestionJobId:guid}")]
    [Authorize(LegalTechPermissions.AIAssist.ReviewSuggestions)]
    public async Task<ListResultDto<ExtractionSuggestionDto>> GetExtractionByJobAsync(Guid ingestionJobId)
    {
        return await _extractionAppService.GetByJobAsync(ingestionJobId);
    }

    [HttpPost("extraction/{id:guid}/decide")]
    [Authorize(LegalTechPermissions.AIAssist.ReviewSuggestions)]
    public async Task<ExtractionSuggestionDto> DecideExtractionAsync(Guid id, [FromBody] DecideSuggestionInput input)
    {
        return await _extractionAppService.DecideAsync(id, input);
    }

    [HttpGet("risk/{ingestionJobId:guid}")]
    [Authorize(LegalTechPermissions.AIAssist.ReviewSuggestions)]
    public async Task<ListResultDto<RiskAssessmentSuggestionDto>> GetRiskByJobAsync(Guid ingestionJobId)
    {
        return await _riskAppService.GetByJobAsync(ingestionJobId);
    }

    [HttpPost("risk/{id:guid}/decide")]
    [Authorize(LegalTechPermissions.AIAssist.ReviewSuggestions)]
    public async Task<RiskAssessmentSuggestionDto> DecideRiskAsync(Guid id, [FromBody] DecideSuggestionInput input)
    {
        return await _riskAppService.DecideAsync(id, input);
    }
}
