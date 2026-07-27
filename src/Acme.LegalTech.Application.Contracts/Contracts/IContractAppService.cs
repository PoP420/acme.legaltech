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
}
