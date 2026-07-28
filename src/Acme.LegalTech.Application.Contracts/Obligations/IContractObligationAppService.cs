using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Obligations;

public interface IContractObligationAppService : ICrudAppService<
    ContractObligationDto,
    Guid,
    ContractObligationGetListInput,
    ContractObligationCreateDto,
    ContractObligationUpdateDto>
{
    Task<ContractObligationDto> CompleteAsync(Guid id);
    Task<ContractObligationDto> DeferAsync(Guid id);
}