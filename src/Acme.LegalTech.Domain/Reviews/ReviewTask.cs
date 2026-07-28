using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Reviews;

public class ReviewTask : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ReviewCaseId { get; protected set; }
    public ReviewCase ReviewCase { get; protected set; } = null!;
    public string Title { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = ReviewTaskStatus.Pending.ToString();
    public Guid? AssignedUserId { get; protected set; }
    public DateTime? DueDate { get; protected set; }
    public int SortOrder { get; protected set; }

    public ReviewTask() { }

    public ReviewTask(
        Guid id,
        Guid? tenantId,
        Guid reviewCaseId,
        string title,
        Guid? assignedUserId = null,
        DateTime? dueDate = null,
        int sortOrder = 0)
        : base(id)
    {
        TenantId = tenantId;
        ReviewCaseId = reviewCaseId;
        Title = title;
        AssignedUserId = assignedUserId;
        DueDate = dueDate;
        SortOrder = sortOrder;
        Status = ReviewTaskStatus.Pending.ToString();
    }

    public void AssignTo(Guid userId)
    {
        AssignedUserId = userId;
        Status = ReviewTaskStatus.InProgress.ToString();
    }

    public void Complete()
    {
        Status = ReviewTaskStatus.Completed.ToString();
    }

    public void Reassign(Guid userId)
    {
        AssignedUserId = userId;
        Status = ReviewTaskStatus.Pending.ToString();
    }
}

public enum ReviewTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3
}