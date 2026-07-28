using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Clauses;

public class ClauseTemplateUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? TaxonomyId { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? RiskLevel { get; set; }
}