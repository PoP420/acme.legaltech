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

    public ContractAppService(IRepository<Contract, Guid> repository)
        : base(repository)
    {
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

    protected override ContractDto MapToGetListOutputDto(Contract entity)
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
        return await base.CreateAsync(input);
    }

    [Authorize(LegalTechPermissions.Contracts.Edit)]
    public override async Task<ContractDto> UpdateAsync(Guid id, ContractUpdateDto input)
    {
        return await base.UpdateAsync(id, input);
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
}
