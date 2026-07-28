using System;
using Acme.LegalTech.Common;

namespace Acme.LegalTech.Playbooks;

public class PlaybookEvaluationResultDto
{
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public RuleSeverity Severity { get; set; }
    public bool Matched { get; set; }
    public string? MatchSpan { get; set; }
    public string? Rationale { get; set; }
    public bool IsPreferred { get; set; }
    public bool IsFallback { get; set; }
    public bool IsProhibited { get; set; }
}