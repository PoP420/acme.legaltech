using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Reviews;

public class ApprovalStep : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ReviewCaseId { get; protected set; }
    public ReviewCase ReviewCase { get; protected set; } = null!;
    public string Name { get; protected set; } = string.Empty;
    public int StepOrder { get; protected set; }
    public string Status { get; protected set; } = ApprovalStepStatus.Pending.ToString();
    public Guid? ApproverUserId { get; protected set; }
    public DateTime? CompletedAt { get; protected set; }
    public string? Comments { get; protected set; }
    public bool IsRequired { get; protected set; }

    public ApprovalStep() { }

    public ApprovalStep(
        Guid id,
        Guid? tenantId,
        Guid reviewCaseId,
        string name,
        int stepOrder,
        bool isRequired = true)
        : base(id)
    {
        TenantId = tenantId;
        ReviewCaseId = reviewCaseId;
        Name = name;
        StepOrder = stepOrder;
        IsRequired = isRequired;
        Status = ApprovalStepStatus.Pending.ToString();
    }

    public void Approve(Guid approverId, string? comments = null)
    {
        ApproverUserId = approverId;
        Status = ApprovalStepStatus.Approved.ToString();
        CompletedAt = DateTime.UtcNow;
        Comments = comments;
    }

    public void Reject(Guid approverId, string? comments = null)
    {
        ApproverUserId = approverId;
        Status = ApprovalStepStatus.Rejected.ToString();
        CompletedAt = DateTime.UtcNow;
        Comments = comments;
    }
}

public enum ApprovalStepStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3
}