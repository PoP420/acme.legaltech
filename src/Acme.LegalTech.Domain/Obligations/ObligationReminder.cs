using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Obligations;

public class ObligationReminder : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ObligationId { get; protected set; }
    public ContractObligation Obligation { get; protected set; } = null!;
    public DateTime ReminderDate { get; protected set; }
    public string ReminderType { get; protected set; } = "DueSoon";
    public bool IsSent { get; protected set; }
    public DateTime? SentAt { get; protected set; }
    public string? SentToUserId { get; protected set; }
    public string? Message { get; protected set; }

    public ObligationReminder() { }

    public ObligationReminder(
        Guid id,
        Guid? tenantId,
        Guid obligationId,
        DateTime reminderDate,
        string reminderType,
        string? message = null)
        : base(id)
    {
        TenantId = tenantId;
        ObligationId = obligationId;
        ReminderDate = reminderDate;
        ReminderType = reminderType;
        Message = message;
        IsSent = false;
    }

    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
    }
}

public enum ReminderType
{
    DueSoon = 0,
    Overdue = 1,
    Completed = 2,
    Custom = 3
}