using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ApprovalStepDto : EntityDto<Guid>
{
    public Guid ReviewCaseId { get; set; }
    public string ReviewCaseTitle { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid? ApproverUserId { get; set; }
    public string? ApproverUserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Comments { get; set; }
    public bool IsRequired { get; set; }
}