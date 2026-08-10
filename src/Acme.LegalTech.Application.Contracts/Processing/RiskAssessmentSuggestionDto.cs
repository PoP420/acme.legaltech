using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Processing;

public class RiskAssessmentSuggestionDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid IngestionJobId { get; set; }
    public Guid ContractId { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RuleId { get; set; }
    public string? ProviderName { get; set; }
}
