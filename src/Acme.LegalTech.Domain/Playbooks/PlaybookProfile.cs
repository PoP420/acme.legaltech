using System;
using System.Collections.Generic;
using System.Linq;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Playbooks;

public class PlaybookProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public bool IsActive { get; protected set; }
    public int Version { get; protected set; }

    public ICollection<PlaybookRule> Rules { get; protected set; } = new List<PlaybookRule>();

    public PlaybookProfile() { }

    public PlaybookProfile(
        Guid id,
        Guid? tenantId,
        string name,
        string? description = null)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsActive = true;
        Version = 1;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
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

    public void AddRule(PlaybookRule rule)
    {
        Rules.Add(rule);
    }

    public void RemoveRule(Guid ruleId)
    {
        var rule = Rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            Rules.Remove(rule);
        }
    }
}