using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Playbooks;

public class PlaybookRuleDto : EntityDto<Guid>
{
    public Guid PlaybookId { get; set; }
    public string PlaybookName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClausePattern { get; set; } = string.Empty;
    public RuleSeverity Severity { get; set; }
    public string? Rationale { get; set; }
    public bool IsPreferred { get; set; }
    public bool IsFallback { get; set; }
    public bool IsProhibited { get; set; }
    public int SortOrder { get; set; }
}