using System;
using Acme.LegalTech.Common;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class ContractGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ContractStatus? Status { get; set; }
    public string? Category { get; set; }
    public Guid? OwnerUserId { get; set; }
}
