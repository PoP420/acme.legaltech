using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Obligations;
using Acme.LegalTech.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Authorization;

namespace Acme.LegalTech.Obligations;

public class ContractObligationAppService :
    CrudAppService<
        ContractObligation,
        ContractObligationDto,
        Guid,
        ContractObligationGetListInput,
        ContractObligationCreateDto,
        ContractObligationUpdateDto>,
    IContractObligationAppService
{
    private readonly IRepository<ContractObligation, Guid> _repository;
    private readonly IRepository<CompletionEvidence, Guid> _evidenceRepository;
    private readonly ICurrentTenant _currentTenant;

    public ContractObligationAppService(
        IRepository<ContractObligation, Guid> repository,
        IRepository<CompletionEvidence, Guid> evidenceRepository,
        ICurrentTenant currentTenant)
        : base(repository)
    {
        _repository = repository;
        _evidenceRepository = evidenceRepository;
        _currentTenant = currentTenant;
    }

    protected override ContractObligation MapToEntity(ContractObligationCreateDto createInput)
    {
        return new ContractObligation(
            Guid.NewGuid(),
            _currentTenant.Id,
            createInput.ContractId,
            createInput.Title,
            createInput.Description,
            createInput.DueDate,
            createInput.SourceClauseReference,
            createInput.IsRecurring,
            createInput.RecurrencePattern,
            createInput.Priority);
    }

    protected override void MapToEntity(ContractObligationUpdateDto updateInput, ContractObligation entity)
    {
        entity.Update(updateInput.Title, updateInput.Description, updateInput.DueDate, updateInput.Priority);
    }

    protected override ContractObligationDto MapToGetOutputDto(ContractObligation entity)
    {
        var evidence = _evidenceRepository.GetListAsync(e => e.ObligationId == entity.Id).Result;

        return new ContractObligationDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ContractId = entity.ContractId,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            DueDate = entity.DueDate,
            CompletedAt = entity.CompletedAt,
            SourceClauseReference = entity.SourceClauseReference,
            IsRecurring = entity.IsRecurring,
            RecurrencePattern = entity.RecurrencePattern,
            Priority = entity.Priority,
            EvidenceCount = evidence.Count,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId
        };
    }

    protected override ContractObligationDto MapToGetListOutputDto(ContractObligation entity)
    {
        return MapToGetOutputDto(entity);
    }

    protected override async Task<IQueryable<ContractObligation>> CreateFilteredQueryAsync(ContractObligationGetListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (input.Filter.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(o => o.Title.Contains(input.Filter!) || o.Description.Contains(input.Filter!));
        }

        if (input.Status.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(o => o.Status == input.Status);
        }

        if (input.ContractId.HasValue)
        {
            query = query.Where(o => o.ContractId == input.ContractId.Value);
        }

        if (input.DueDateFrom.HasValue)
        {
            query = query.Where(o => o.DueDate >= input.DueDateFrom.Value);
        }

        if (input.DueDateTo.HasValue)
        {
            query = query.Where(o => o.DueDate <= input.DueDateTo.Value);
        }

        return query;
    }

    [Authorize(LegalTechPermissions.Obligations.Default)]
    public override async Task<ContractObligationDto> CreateAsync(ContractObligationCreateDto input)
    {
        return await base.CreateAsync(input);
    }

    [Authorize(LegalTechPermissions.Obligations.Manage)]
    public override async Task<ContractObligationDto> UpdateAsync(Guid id, ContractObligationUpdateDto input)
    {
        return await base.UpdateAsync(id, input);
    }

    [Authorize(LegalTechPermissions.Obligations.Complete)]
    public async Task<ContractObligationDto> CompleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        entity.MarkAsComplete();
        await Repository.UpdateAsync(entity);
        return MapToGetOutputDto(entity);
    }

    [Authorize(LegalTechPermissions.Obligations.Manage)]
    public async Task<ContractObligationDto> DeferAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        entity.MarkAsDeferred();
        await Repository.UpdateAsync(entity);
        return MapToGetOutputDto(entity);
    }
}