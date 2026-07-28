using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Clauses;

public class ClauseTaxonomy : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public Guid? ParentId { get; protected set; }
    public ClauseTaxonomy? Parent { get; protected set; }
    public ICollection<ClauseTaxonomy> Children { get; protected set; } = new List<ClauseTaxonomy>();
    public int SortOrder { get; protected set; }
    public bool IsActive { get; protected set; }

    public ClauseTaxonomy() { }

    public ClauseTaxonomy(
        Guid id,
        Guid? tenantId,
        string name,
        string? description = null,
        Guid? parentId = null,
        int sortOrder = 0)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ParentId = parentId;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public void Update(string name, string? description, int sortOrder)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
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