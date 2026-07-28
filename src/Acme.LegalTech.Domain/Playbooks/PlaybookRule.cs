using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Playbooks;

public class PlaybookRule : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid PlaybookId { get; protected set; }
    public PlaybookProfile Playbook { get; protected set; } = null!;
    public string Name { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string ClausePattern { get; protected set; } = string.Empty;
    public RuleSeverity Severity { get; protected set; }
    public string? Rationale { get; protected set; }
    public bool IsPreferred { get; protected set; }
    public bool IsFallback { get; protected set; }
    public bool IsProhibited { get; protected set; }
    public int SortOrder { get; protected set; }

    public PlaybookRule() { }

    public PlaybookRule(
        Guid id,
        Guid? tenantId,
        Guid playbookId,
        string name,
        string description,
        string clausePattern,
        RuleSeverity severity,
        string? rationale = null,
        bool isPreferred = false,
        bool isFallback = false,
        bool isProhibited = false,
        int sortOrder = 0)
        : base(id)
    {
        TenantId = tenantId;
        PlaybookId = playbookId;
        Name = name;
        Description = description;
        ClausePattern = clausePattern;
        Severity = severity;
        Rationale = rationale;
        IsPreferred = isPreferred;
        IsFallback = isFallback;
        IsProhibited = isProhibited;
        SortOrder = sortOrder;
    }

    public void Update(string name, string description, string clausePattern, RuleSeverity severity, string? rationale, bool isPreferred, bool isFallback, bool isProhibited, int sortOrder)
    {
        Name = name;
        Description = description;
        ClausePattern = clausePattern;
        Severity = severity;
        Rationale = rationale;
        IsPreferred = isPreferred;
        IsFallback = isFallback;
        IsProhibited = isProhibited;
        SortOrder = sortOrder;
    }
}