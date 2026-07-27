using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class ContractTagDto : EntityDto<Guid>
{
    public Guid ContractId { get; set; }
    public string Name { get; set; } = string.Empty;
}
