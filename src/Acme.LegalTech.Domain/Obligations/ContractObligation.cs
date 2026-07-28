using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Obligations;

public class ContractObligation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = ObligationStatus.Pending.ToString();
    public DateTime? DueDate { get; protected set; }
    public DateTime? CompletedAt { get; protected set; }
    public Guid? CompletionEvidenceId { get; protected set; }
    public string? SourceClauseReference { get; protected set; }
    public bool IsRecurring { get; protected set; }
    public string? RecurrencePattern { get; protected set; }
    public int Priority { get; protected set; }

    public ICollection<CompletionEvidence> Evidence { get; protected set; } = new List<CompletionEvidence>();

    public ContractObligation() { }

    public ContractObligation(
        Guid id,
        Guid? tenantId,
        Guid contractId,
        string title,
        string description,
        DateTime? dueDate = null,
        string? sourceClauseReference = null,
        bool isRecurring = false,
        string? recurrencePattern = null,
        int priority = 0,
        string? status = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        SourceClauseReference = sourceClauseReference;
        IsRecurring = isRecurring;
        RecurrencePattern = recurrencePattern;
        Priority = priority;
        Status = status ?? ObligationStatus.Pending.ToString();
    }

    public void MarkAsComplete()
    {
        Status = ObligationStatus.Completed.ToString();
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsDeferred()
    {
        Status = ObligationStatus.Deferred.ToString();
    }

    public void MarkAsOverdue()
    {
        Status = ObligationStatus.Overdue.ToString();
    }

    public void Update(string title, string description, DateTime? dueDate, int priority)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
    }
}

public enum ObligationStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Deferred = 3,
    Overdue = 4
}