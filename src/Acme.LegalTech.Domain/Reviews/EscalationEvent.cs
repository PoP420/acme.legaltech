using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Reviews;

public class EscalationEvent : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ReviewCaseId { get; protected set; }
    public ReviewCase ReviewCase { get; protected set; } = null!;
    public string Reason { get; protected set; } = string.Empty;
    public string Severity { get; protected set; } = EscalationSeverity.Medium.ToString();
    public Guid? EscalatedByUserId { get; protected set; }
    public DateTime EscalatedAt { get; protected set; }
    public DateTime? ResolvedAt { get; protected set; }
    public string? Resolution { get; protected set; }
    public Guid? ResolvedByUserId { get; protected set; }

    public EscalationEvent() { }

    public EscalationEvent(
        Guid id,
        Guid? tenantId,
        Guid reviewCaseId,
        string reason,
        string severity,
        Guid? escalatedByUserId)
        : base(id)
    {
        TenantId = tenantId;
        ReviewCaseId = reviewCaseId;
        Reason = reason;
        Severity = severity;
        EscalatedByUserId = escalatedByUserId;
        EscalatedAt = DateTime.UtcNow;
    }

    public void Resolve(Guid resolvedByUserId, string? resolution = null)
    {
        ResolvedByUserId = resolvedByUserId;
        ResolvedAt = DateTime.UtcNow;
        Resolution = resolution;
    }
}

public enum EscalationSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}