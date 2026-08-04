using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Contracts;

public interface IContractAppService :
    ICrudAppService<
        ContractDto,
        Guid,
        ContractGetListInput,
        ContractCreateDto,
        ContractUpdateDto>
{
    Task ChangeStatusAsync(Guid id, ContractChangeStatusDto input);
    Task<ContractSignatoryDto> AddSignatoryAsync(Guid id, AddSignatoryDto input);
    Task<VariationOrderDto> AddVariationOrderAsync(Guid id, AddVariationOrderDto input);
    Task<ApprovalAuthorityResultDto> GetApprovalAuthorityAsync(Guid id, decimal amount);
    Task<ContractComplianceDto> GetContractComplianceAsync(Guid id);
}
