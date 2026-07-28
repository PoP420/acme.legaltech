using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Obligations;

public class ContractObligationGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}