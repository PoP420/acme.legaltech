using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Acme.LegalTech.Processing;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Processing;

[RemoteService(false)]
[Authorize(LegalTechPermissions.AIAssist.ReviewSuggestions)]
public class SuggestionAppService : ApplicationService, IExtractionSuggestionAppService, IRiskAssessmentSuggestionAppService
{
    private readonly IRepository<ExtractionSuggestion, Guid> _extractionRepository;
    private readonly IRepository<RiskAssessmentSuggestion, Guid> _riskRepository;
    private readonly IRepository<SuggestionDecision, Guid> _decisionRepository;
    private readonly ICurrentTenant _currentTenant;

    public SuggestionAppService(
        IRepository<ExtractionSuggestion, Guid> extractionRepository,
        IRepository<RiskAssessmentSuggestion, Guid> riskRepository,
        IRepository<SuggestionDecision, Guid> decisionRepository,
        ICurrentTenant currentTenant)
    {
        _extractionRepository = extractionRepository;
        _riskRepository = riskRepository;
        _decisionRepository = decisionRepository;
        _currentTenant = currentTenant;
    }

    async Task<ListResultDto<ExtractionSuggestionDto>> IExtractionSuggestionAppService.GetByJobAsync(Guid ingestionJobId)
    {
        var query = await _extractionRepository.GetQueryableAsync();
        var suggestions = await AsyncExecuter.ToListAsync(
            query.Where(s => s.IngestionJobId == ingestionJobId));

        return new ListResultDto<ExtractionSuggestionDto>(
            ObjectMapper.Map<List<ExtractionSuggestion>, List<ExtractionSuggestionDto>>(suggestions));
    }

    async Task<ExtractionSuggestionDto> IExtractionSuggestionAppService.DecideAsync(Guid id, DecideSuggestionInput input)
    {
        var suggestion = await _extractionRepository.GetAsync(id);

        switch (input.Decision.ToLowerInvariant())
        {
            case "accept":
                suggestion.Accept(input.CorrectedValue);
                break;
            case "reject":
                suggestion.Reject();
                break;
            case "correct":
                suggestion.MarkAsCorrected(input.CorrectedValue ?? string.Empty);
                break;
            default:
                throw new BusinessException("LegalTech:AI:InvalidDecision");
        }

        await _extractionRepository.UpdateAsync(suggestion);

        var decision = new SuggestionDecision(
            Guid.NewGuid(),
            _currentTenant.Id,
            id,
            "Extraction",
            CurrentUser.Id,
            input.Decision,
            input.CorrectedValue,
            input.Comment);

        await _decisionRepository.InsertAsync(decision);

        return ObjectMapper.Map<ExtractionSuggestion, ExtractionSuggestionDto>(suggestion);
    }

    async Task<ListResultDto<RiskAssessmentSuggestionDto>> IRiskAssessmentSuggestionAppService.GetByJobAsync(Guid ingestionJobId)
    {
        var query = await _riskRepository.GetQueryableAsync();
        var suggestions = await AsyncExecuter.ToListAsync(
            query.Where(s => s.IngestionJobId == ingestionJobId));

        return new ListResultDto<RiskAssessmentSuggestionDto>(
            ObjectMapper.Map<List<RiskAssessmentSuggestion>, List<RiskAssessmentSuggestionDto>>(suggestions));
    }

    async Task<RiskAssessmentSuggestionDto> IRiskAssessmentSuggestionAppService.DecideAsync(Guid id, DecideSuggestionInput input)
    {
        var suggestion = await _riskRepository.GetAsync(id);

        switch (input.Decision.ToLowerInvariant())
        {
            case "accept":
                suggestion.Accept();
                break;
            case "reject":
                suggestion.Reject();
                break;
            case "correct":
                suggestion.MarkAsCorrected(input.CorrectedValue ?? string.Empty, input.CorrectedValue ?? string.Empty);
                break;
            default:
                throw new BusinessException("LegalTech:AI:InvalidDecision");
        }

        await _riskRepository.UpdateAsync(suggestion);

        var decision = new SuggestionDecision(
            Guid.NewGuid(),
            _currentTenant.Id,
            id,
            "RiskAssessment",
            CurrentUser.Id,
            input.Decision,
            input.CorrectedValue,
            input.Comment);

        await _decisionRepository.InsertAsync(decision);

        return ObjectMapper.Map<RiskAssessmentSuggestion, RiskAssessmentSuggestionDto>(suggestion);
    }
}
