using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ReviewCaseGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public int? Priority { get; set; }
}