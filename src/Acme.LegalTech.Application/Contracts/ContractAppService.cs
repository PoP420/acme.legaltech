using System;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Common;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

[Authorize(LegalTechPermissions.Contracts.Default)]
public class ContractAppService :
    CrudAppService<
        Contract,
        ContractDto,
        Guid,
        ContractGetListInput,
        ContractCreateDto,
        ContractUpdateDto>,
    IContractAppService
{
    private readonly LegalTechApplicationMappers _mappers = new();
    private readonly IRepository<ContractTag, Guid> _contractTagRepository;
    private readonly IRepository<CounterpartyReference, Guid> _counterpartyReferenceRepository;
    private readonly IRepository<ContractSignatory, Guid> _contractSignatoryRepository;
    private readonly IRepository<VariationOrder, Guid> _variationOrderRepository;
    private readonly IRepository<GovernmentApprovalTier, Guid> _approvalTierRepository;
    private readonly ICurrentTenant _currentTenant;

    public ContractAppService(
        IRepository<Contract, Guid> repository,
        IRepository<ContractTag, Guid> contractTagRepository,
        IRepository<CounterpartyReference, Guid> counterpartyReferenceRepository,
        IRepository<ContractSignatory, Guid> contractSignatoryRepository,
        IRepository<VariationOrder, Guid> variationOrderRepository,
        IRepository<GovernmentApprovalTier, Guid> approvalTierRepository,
        ICurrentTenant currentTenant)
        : base(repository)
    {
        _contractTagRepository = contractTagRepository;
        _counterpartyReferenceRepository = counterpartyReferenceRepository;
        _contractSignatoryRepository = contractSignatoryRepository;
        _variationOrderRepository = variationOrderRepository;
        _approvalTierRepository = approvalTierRepository;
        _currentTenant = currentTenant;
    }

    public override async Task<ContractDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var dto = MapToGetOutputDto(entity);

        dto.Tags = (await _contractTagRepository.GetListAsync(t => t.ContractId == id))
            .Select(t => _mappers.MapToContractTagDto(t))
            .ToList();

        dto.Counterparties = (await _counterpartyReferenceRepository.GetListAsync(c => c.ContractId == id))
            .Select(c => _mappers.MapToCounterpartyReferenceDto(c))
            .ToList();

        dto.Signatories = (await _contractSignatoryRepository.GetListAsync(s => s.ContractId == id))
            .Select(s => _mappers.MapToContractSignatoryDto(s))
            .OrderBy(s => s.Order)
            .ToList();

        dto.VariationOrders = (await _variationOrderRepository.GetListAsync(v => v.ContractId == id))
            .Select(v => _mappers.MapToVariationOrderDto(v))
            .OrderBy(v => v.CumulativeAmount)
            .ToList();

        var tiers = await _approvalTierRepository.GetListAsync();
        var computedTier = tiers.Any() ? entity.ComputeApprovingAuthority(entity.ContractValue ?? 0, tiers) : null;
        dto.CurrentAuthority = computedTier != null ? new ApprovalAuthorityResultDto
        {
            AuthorityTitle = entity.LastApprovalAuthorityTitle ?? computedTier.AuthorityTitle,
            RequiresNedaReview = entity.LastApprovalRequiresNeda,
            RequiresPresident = entity.LastApprovalRequiresPresident,
            AllowableVariationPercent = computedTier.AllowableVariationPercent
        } : null;

        return dto;
    }

    protected override Contract MapToEntity(ContractCreateDto createInput)
    {
        return _mappers.MapToContract(createInput);
    }

    protected override void MapToEntity(ContractUpdateDto updateInput, Contract entity)
    {
        _mappers.MapToContract(updateInput, entity);
    }

    protected override ContractDto MapToGetOutputDto(Contract entity)
    {
        return _mappers.MapToContractDto(entity);
    }

    protected override async Task<IQueryable<Contract>> CreateFilteredQueryAsync(ContractGetListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (input.Filter.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c =>
                c.Title.Contains(input.Filter!) ||
                c.CounterpartyName.Contains(input.Filter!));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(c => c.Status == input.Status.Value);
        }

        if (input.Category.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c => c.Category == input.Category);
        }

        if (input.OwnerUserId.HasValue)
        {
            query = query.Where(c => c.OwnerUserId == input.OwnerUserId.Value);
        }

        return query;
    }

    [Authorize(LegalTechPermissions.Contracts.Create)]
    public override async Task<ContractDto> CreateAsync(ContractCreateDto input)
    {
        var contract = MapToEntity(input);
        await Repository.InsertAsync(contract, autoSave: true);

        var tenantId = _currentTenant.Id;

        if (input.Tags?.Any() == true)
        {
            foreach (var tag in input.Tags)
            {
                await _contractTagRepository.InsertAsync(
                    new ContractTag(Guid.NewGuid(), tenantId, contract.Id, tag.Name),
                    autoSave: true);
            }
        }

        if (input.Counterparties?.Any() == true)
        {
            foreach (var cp in input.Counterparties)
            {
                await _counterpartyReferenceRepository.InsertAsync(
                    new CounterpartyReference(Guid.NewGuid(), tenantId, contract.Id, cp.Name, cp.ExternalReference),
                    autoSave: true);
            }
        }

        return await GetAsync(contract.Id);
    }

    [Authorize(LegalTechPermissions.Contracts.Edit)]
    public override async Task<ContractDto> UpdateAsync(Guid id, ContractUpdateDto input)
    {
        var contract = await Repository.GetAsync(id);
        MapToEntity(input, contract);

        var tenantId = _currentTenant.Id;

        var existingTags = await _contractTagRepository.GetListAsync(t => t.ContractId == id);
        foreach (var tag in existingTags)
        {
            await _contractTagRepository.DeleteAsync(tag, autoSave: true);
        }
        if (input.Tags?.Any() == true)
        {
            foreach (var tag in input.Tags)
            {
                await _contractTagRepository.InsertAsync(
                    new ContractTag(Guid.NewGuid(), tenantId, contract.Id, tag.Name),
                    autoSave: true);
            }
        }

        var existingCps = await _counterpartyReferenceRepository.GetListAsync(c => c.ContractId == id);
        foreach (var cp in existingCps)
        {
            await _counterpartyReferenceRepository.DeleteAsync(cp, autoSave: true);
        }
        if (input.Counterparties?.Any() == true)
        {
            foreach (var cp in input.Counterparties)
            {
                await _counterpartyReferenceRepository.InsertAsync(
                    new CounterpartyReference(Guid.NewGuid(), tenantId, contract.Id, cp.Name, cp.ExternalReference),
                    autoSave: true);
            }
        }

        await Repository.UpdateAsync(contract, autoSave: true);
        return await GetAsync(contract.Id);
    }

    [Authorize(LegalTechPermissions.Contracts.Edit)]
    public override async Task DeleteAsync(Guid id)
    {
        await base.DeleteAsync(id);
    }

    [Authorize(LegalTechPermissions.Contracts.ChangeStatus)]
    public async Task ChangeStatusAsync(Guid id, ContractChangeStatusDto input)
    {
        var contract = await Repository.GetAsync(id);
        switch (input.TargetStatus)
        {
            case ContractStatus.Active:
                contract.Activate();
                break;
            case ContractStatus.Expired:
                contract.Expire();
                break;
            case ContractStatus.Terminated:
                contract.Terminate();
                break;
            default:
                throw new BusinessException("LegalTech:Contract:InvalidStatusTransition")
                {
                    Data =
                    {
                        ["From"] = contract.Status.ToString(),
                        ["To"] = input.TargetStatus.ToString()
                    }
                };
        }

        await Repository.UpdateAsync(contract);
    }

    [Authorize(LegalTechPermissions.Contracts.ManageSignatories)]
    public async Task<ContractSignatoryDto> AddSignatoryAsync(Guid id, AddSignatoryDto input)
    {
        var contract = await Repository.GetAsync(id);
        var tenantId = _currentTenant.Id;

        var signatory = new ContractSignatory(
            Guid.NewGuid(),
            tenantId,
            id,
            input.Role,
            input.PartyType,
            input.PartyId,
            input.GovernmentAgency,
            input.Capacity,
            input.Order,
            input.SignedOn);

        contract.AddSignatory(signatory);
        await _contractSignatoryRepository.InsertAsync(signatory, autoSave: true);

        return _mappers.MapToContractSignatoryDto(signatory);
    }

    [Authorize(LegalTechPermissions.Contracts.Amend)]
    public async Task<VariationOrderDto> AddVariationOrderAsync(Guid id, AddVariationOrderDto input)
    {
        var contract = await Repository.GetAsync(id);
        var tenantId = _currentTenant.Id;
        var tiers = await _approvalTierRepository.GetListAsync();

        var variationOrder = new VariationOrder(
            Guid.NewGuid(),
            tenantId,
            id,
            input.Description,
            input.Amount,
            input.Amount);

        contract.AddVariationOrder(variationOrder, tiers);
        await _variationOrderRepository.InsertAsync(variationOrder, autoSave: true);

        return _mappers.MapToVariationOrderDto(variationOrder);
    }

    [Authorize(LegalTechPermissions.Contracts.ViewGovFields)]
    public async Task<ApprovalAuthorityResultDto> GetApprovalAuthorityAsync(Guid id, decimal amount)
    {
        var contract = await Repository.GetAsync(id);
        var tiers = await _approvalTierRepository.GetListAsync();

        var tier = contract.ComputeApprovingAuthority(amount, tiers);

        return new ApprovalAuthorityResultDto
        {
            AuthorityTitle = tier.AuthorityTitle,
            RequiresNedaReview = tier.RequiresNedaReview,
            RequiresPresident = tier.RequiresPresident,
            AllowableVariationPercent = tier.AllowableVariationPercent,
            LastApprovalAuthorityTitle = contract.LastApprovalAuthorityTitle,
            LastApprovalRequiresNeda = contract.LastApprovalRequiresNeda,
            LastApprovalRequiresPresident = contract.LastApprovalRequiresPresident
        };
    }

    [Authorize(LegalTechPermissions.Contracts.ViewGovFields)]
    public async Task<ContractComplianceDto> GetContractComplianceAsync(Guid id)
    {
        var contract = await Repository.GetAsync(id);
        var signatories = await _contractSignatoryRepository.GetListAsync(s => s.ContractId == id);
        var variationOrders = await _variationOrderRepository.GetListAsync(v => v.ContractId == id);
        var tiers = await _approvalTierRepository.GetListAsync();
        var computedTier = tiers.Any() ? contract.ComputeApprovingAuthority(contract.ContractValue ?? 0, tiers) : null;

        return new ContractComplianceDto
        {
            DocumentNumber = contract.DocumentNumber,
            DocumentSeries = contract.DocumentSeries,
            DocumentYear = contract.DocumentYear,
            Classification = contract.Classification,
            RetentionUntil = contract.RetentionUntil,
            ContractValue = contract.ContractValue,
            Signatories = signatories.Select(s => _mappers.MapToContractSignatoryDto(s)).OrderBy(s => s.Order).ToList(),
            VariationOrders = variationOrders.Select(v => _mappers.MapToVariationOrderDto(v)).OrderBy(v => v.CumulativeAmount).ToList(),
            CurrentAuthority = computedTier != null ? new ApprovalAuthorityResultDto
            {
                AuthorityTitle = contract.LastApprovalAuthorityTitle ?? computedTier.AuthorityTitle,
                RequiresNedaReview = contract.LastApprovalRequiresNeda,
                RequiresPresident = contract.LastApprovalRequiresPresident,
                AllowableVariationPercent = computedTier.AllowableVariationPercent
            } : null
        };
    }
}
