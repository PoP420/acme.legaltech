using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Reviews;

public class ReviewCase : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public Guid ContractId { get; protected set; }
    public string Status { get; protected set; } = ReviewCaseStatus.Draft.ToString();
    public Guid? AssignedUserId { get; protected set; }
    public DateTime? DueDate { get; protected set; }
    public string? Summary { get; protected set; }
    public int Priority { get; protected set; }

    public ICollection<ReviewTask> Tasks { get; protected set; } = new List<ReviewTask>();
    public ICollection<ReviewComment> Comments { get; protected set; } = new List<ReviewComment>();
    public ICollection<EscalationEvent> Escalations { get; protected set; } = new List<EscalationEvent>();

    public ReviewCase() { }

    public ReviewCase(
        Guid id,
        Guid? tenantId,
        string title,
        Guid contractId,
        Guid? assignedUserId = null,
        int priority = 0,
        string? summary = null,
        DateTime? dueDate = null)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        ContractId = contractId;
        AssignedUserId = assignedUserId;
        Priority = priority;
        Summary = summary;
        DueDate = dueDate;
        Status = ReviewCaseStatus.Draft.ToString();
    }

    public void AssignTo(Guid userId)
    {
        AssignedUserId = userId;
        Status = ReviewCaseStatus.InProgress.ToString();
    }

    public void AddComment(ReviewComment comment)
    {
        Comments.Add(comment);
    }

    public void Escalate(EscalationEvent escalation)
    {
        Escalations.Add(escalation);
        Status = ReviewCaseStatus.Escalated.ToString();
    }

    public void MarkAsComplete()
    {
        Status = ReviewCaseStatus.Completed.ToString();
    }

    public void MarkAsRejected()
    {
        Status = ReviewCaseStatus.Rejected.ToString();
    }

    public void Update(string title, Guid? assignedUserId, int priority, string? summary, DateTime? dueDate)
    {
        Title = title;
        AssignedUserId = assignedUserId;
        Priority = priority;
        Summary = summary;
        DueDate = dueDate;
    }
}

public enum ReviewCaseStatus
{
    Draft = 0,
    InProgress = 1,
    PendingApproval = 2,
    Completed = 3,
    Rejected = 4,
    Escalated = 5
}