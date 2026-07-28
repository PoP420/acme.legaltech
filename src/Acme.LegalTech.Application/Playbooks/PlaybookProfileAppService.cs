using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Clauses;
using Acme.LegalTech.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Authorization;

namespace Acme.LegalTech.Playbooks;

[Authorize(LegalTechPermissions.Playbooks.Default)]
public class PlaybookProfileAppService :
    CrudAppService<
        PlaybookProfile,
        PlaybookProfileDto,
        Guid,
        PagedAndSortedResultRequestDto,
        PlaybookProfileCreateDto,
        PlaybookProfileUpdateDto>,
    IPlaybookProfileAppService
{
    private readonly IRepository<PlaybookProfile, Guid> _playbookRepository;
    private readonly IRepository<PlaybookRule, Guid> _ruleRepository;
    private readonly PlaybookEvaluationService _evaluationService;

    public PlaybookProfileAppService(
        IRepository<PlaybookProfile, Guid> playbookRepository,
        IRepository<PlaybookRule, Guid> ruleRepository,
        PlaybookEvaluationService evaluationService)
        : base(playbookRepository)
    {
        _playbookRepository = playbookRepository;
        _ruleRepository = ruleRepository;
        _evaluationService = evaluationService;
    }

    protected override PlaybookProfile MapToEntity(PlaybookProfileCreateDto createInput)
    {
        return new PlaybookProfile(
            Guid.NewGuid(),
            CurrentTenant.Id,
            createInput.Name,
            createInput.Description);
    }

    protected override void MapToEntity(PlaybookProfileUpdateDto updateInput, PlaybookProfile entity)
    {
        entity.Update(updateInput.Name, updateInput.Description);
    }

    protected override PlaybookProfileDto MapToGetOutputDto(PlaybookProfile entity)
    {
        var rules = entity.Rules.Select(r => new PlaybookRuleDto
        {
            Id = r.Id,
            PlaybookId = r.PlaybookId,
            PlaybookName = entity.Name,
            Name = r.Name,
            Description = r.Description,
            ClausePattern = r.ClausePattern,
            Severity = r.Severity,
            Rationale = r.Rationale,
            IsPreferred = r.IsPreferred,
            IsFallback = r.IsFallback,
            IsProhibited = r.IsProhibited,
            SortOrder = r.SortOrder
        }).ToList();

        return new PlaybookProfileDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Version = entity.Version,
            Rules = rules,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime
        };
    }

    protected override PlaybookProfileDto MapToGetListOutputDto(PlaybookProfile entity)
    {
        return MapToGetOutputDto(entity);
    }

    [Authorize(LegalTechPermissions.Playbooks.Manage)]
    public override async Task<PlaybookProfileDto> CreateAsync(PlaybookProfileCreateDto input)
    {
        return await base.CreateAsync(input);
    }

    [Authorize(LegalTechPermissions.Playbooks.Manage)]
    public override async Task<PlaybookProfileDto> UpdateAsync(Guid id, PlaybookProfileUpdateDto input)
    {
        return await base.UpdateAsync(id, input);
    }

    [Authorize(LegalTechPermissions.Playbooks.Manage)]
    public override async Task DeleteAsync(Guid id)
    {
        await base.DeleteAsync(id);
    }

    [Authorize(LegalTechPermissions.Playbooks.Evaluate)]
    public async Task<PlaybookEvaluationResultDto[]> EvaluateAsync(PlaybookEvaluateInput input)
    {
        return await _evaluationService.EvaluateAsync(input);
    }
}