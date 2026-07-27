using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class CounterpartyReferenceDto : EntityDto<Guid>
{
    public Guid ContractId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
}
