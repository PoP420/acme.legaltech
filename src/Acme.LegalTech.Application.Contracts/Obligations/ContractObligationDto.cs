using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Obligations;

public class ContractObligationDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid ContractId { get; set; }
    public string ContractTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? SourceClauseReference { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int Priority { get; set; }
    public int EvidenceCount { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
}