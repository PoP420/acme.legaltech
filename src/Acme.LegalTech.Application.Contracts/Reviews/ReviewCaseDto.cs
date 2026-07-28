using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ReviewCaseDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ContractId { get; set; }
    public string ContractTitle { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Summary { get; set; }
    public int Priority { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public int EscalationCount { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
}