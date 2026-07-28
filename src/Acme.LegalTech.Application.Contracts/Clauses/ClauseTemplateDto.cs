using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Clauses;

public class ClauseTemplateDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? TaxonomyId { get; set; }
    public string? TaxonomyName { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? RiskLevel { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
}