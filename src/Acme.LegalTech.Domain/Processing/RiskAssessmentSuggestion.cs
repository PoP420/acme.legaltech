using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Processing;

public class RiskAssessmentSuggestion : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid IngestionJobId { get; protected set; }
    public Guid ContractId { get; protected set; }
    public string RiskType { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public string Severity { get; protected set; } = string.Empty;
    public double Confidence { get; protected set; }
    public string Status { get; protected set; } = SuggestionStatus.Pending.ToString();
    public string? RuleId { get; protected set; }
    public string? ProviderName { get; protected set; }

    public IngestionJob? IngestionJob { get; protected set; }

    public RiskAssessmentSuggestion() { }

    public RiskAssessmentSuggestion(
        Guid id,
        Guid? tenantId,
        Guid ingestionJobId,
        Guid contractId,
        string riskType,
        string? description,
        string severity,
        double confidence,
        string? ruleId = null,
        string? providerName = null)
        : base(id)
    {
        TenantId = tenantId;
        IngestionJobId = ingestionJobId;
        ContractId = contractId;
        RiskType = riskType;
        Description = description;
        Severity = severity;
        Confidence = confidence;
        RuleId = ruleId;
        ProviderName = providerName;
        Status = SuggestionStatus.Pending.ToString();
    }

    public void Accept()
    {
        Status = SuggestionStatus.Accepted.ToString();
    }

    public void Reject()
    {
        Status = SuggestionStatus.Rejected.ToString();
    }

    public void MarkAsCorrected(string correctedDescription, string correctedSeverity)
    {
        Status = SuggestionStatus.Corrected.ToString();
        Description = correctedDescription;
        Severity = correctedSeverity;
    }
}
