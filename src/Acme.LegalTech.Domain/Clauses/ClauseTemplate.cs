using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Clauses;

public class ClauseTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public string Content { get; protected set; } = string.Empty;
    public Guid? TaxonomyId { get; protected set; }
    public ClauseTaxonomy? Taxonomy { get; protected set; }
    public bool IsActive { get; protected set; }
    public int Version { get; protected set; }
    public string? Jurisdiction { get; protected set; }
    public string? Category { get; protected set; }
    public string? Tags { get; protected set; }
    public string? RiskLevel { get; protected set; }

    public ClauseTemplate() { }

    public ClauseTemplate(
        Guid id,
        Guid? tenantId,
        string title,
        string content,
        Guid? taxonomyId,
        string? jurisdiction = null,
        string? category = null,
        string? tags = null,
        string? riskLevel = null)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Content = content;
        TaxonomyId = taxonomyId;
        IsActive = true;
        Version = 1;
        Jurisdiction = jurisdiction;
        Category = category;
        Tags = tags;
        RiskLevel = riskLevel;
    }

    public void Update(string title, string content, Guid? taxonomyId, string? jurisdiction, string? category, string? tags, string? riskLevel)
    {
        Title = title;
        Content = content;
        TaxonomyId = taxonomyId;
        Jurisdiction = jurisdiction;
        Category = category;
        Tags = tags;
        RiskLevel = riskLevel;
        Version++;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}