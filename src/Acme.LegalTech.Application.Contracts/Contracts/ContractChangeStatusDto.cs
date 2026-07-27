using Acme.LegalTech.Common;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class ContractChangeStatusDto
{
    public ContractStatus TargetStatus { get; set; }
    public string? ChangeNote { get; set; }
}
