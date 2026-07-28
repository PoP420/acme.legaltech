using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Clauses;
using Acme.LegalTech.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Authorization;

namespace Acme.LegalTech.Clauses;

[Authorize(LegalTechPermissions.Clauses.Default)]
public class ClauseTemplateAppService :
    CrudAppService<
        ClauseTemplate,
        ClauseTemplateDto,
        Guid,
        ClauseGetListInput,
        ClauseTemplateCreateDto,
        ClauseTemplateUpdateDto>,
    IClauseTemplateAppService
{
    private readonly LegalTechApplicationMappers _mappers = new();
    private readonly IRepository<ClauseTemplate, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public ClauseTemplateAppService(
        IRepository<ClauseTemplate, Guid> repository,
        ICurrentTenant currentTenant)
        : base(repository)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    protected override ClauseTemplate MapToEntity(ClauseTemplateCreateDto createInput)
    {
        return new ClauseTemplate(
            Guid.NewGuid(),
            _currentTenant.Id,
            createInput.Title,
            createInput.Content,
            createInput.TaxonomyId,
            createInput.Jurisdiction,
            createInput.Category,
            createInput.Tags,
            createInput.RiskLevel);
    }

    protected override void MapToEntity(ClauseTemplateUpdateDto updateInput, ClauseTemplate entity)
    {
        entity.Update(
            updateInput.Title,
            updateInput.Content,
            updateInput.TaxonomyId,
            updateInput.Jurisdiction,
            updateInput.Category,
            updateInput.Tags,
            updateInput.RiskLevel);
    }

    protected override ClauseTemplateDto MapToGetOutputDto(ClauseTemplate entity)
    {
        return new ClauseTemplateDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Title = entity.Title,
            Content = entity.Content,
            TaxonomyId = entity.TaxonomyId,
            TaxonomyName = entity.Taxonomy?.Name,
            IsActive = entity.IsActive,
            Version = entity.Version,
            Jurisdiction = entity.Jurisdiction,
            Category = entity.Category,
            Tags = entity.Tags,
            RiskLevel = entity.RiskLevel,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    protected override ClauseTemplateDto MapToGetListOutputDto(ClauseTemplate entity)
    {
        return MapToGetOutputDto(entity);
    }

    protected override async Task<IQueryable<ClauseTemplate>> CreateFilteredQueryAsync(ClauseGetListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (input.Filter.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c =>
                c.Title.Contains(input.Filter!) ||
                c.Content.Contains(input.Filter!));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == input.IsActive.Value);
        }

        if (input.TaxonomyId.HasValue)
        {
            query = query.Where(c => c.TaxonomyId == input.TaxonomyId.Value);
        }

        if (input.Category.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c => c.Category == input.Category);
        }

        if (input.Jurisdiction.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c => c.Jurisdiction == input.Jurisdiction);
        }

        return query;
    }

    [Authorize(LegalTechPermissions.Clauses.Manage)]
    public override async Task<ClauseTemplateDto> CreateAsync(ClauseTemplateCreateDto input)
    {
        return await base.CreateAsync(input);
    }

    [Authorize(LegalTechPermissions.Clauses.Manage)]
    public override async Task<ClauseTemplateDto> UpdateAsync(Guid id, ClauseTemplateUpdateDto input)
    {
        return await base.UpdateAsync(id, input);
    }

    [Authorize(LegalTechPermissions.Clauses.Manage)]
    public override async Task DeleteAsync(Guid id)
    {
        await base.DeleteAsync(id);
    }
}