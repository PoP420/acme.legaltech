using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class EscalationEventDto : EntityDto<Guid>
{
    public Guid ReviewCaseId { get; set; }
    public string ReviewCaseTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public Guid? EscalatedByUserId { get; set; }
    public string? EscalatedByUserName { get; set; }
    public DateTime EscalatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public Guid? ResolvedByUserId { get; set; }
}