using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Processing;

public class ExtractionSuggestion : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid IngestionJobId { get; protected set; }
    public Guid ContractDocumentVersionId { get; protected set; }
    public string FieldName { get; protected set; } = string.Empty;
    public string? SuggestedValue { get; protected set; }
    public string? OriginalValue { get; protected set; }
    public double Confidence { get; protected set; }
    public string Status { get; protected set; } = SuggestionStatus.Pending.ToString();
    public string? SourceSpan { get; protected set; }
    public string? ProviderName { get; protected set; }

    public IngestionJob? IngestionJob { get; protected set; }

    public ExtractionSuggestion() { }

    public ExtractionSuggestion(
        Guid id,
        Guid? tenantId,
        Guid ingestionJobId,
        Guid contractDocumentVersionId,
        string fieldName,
        string? suggestedValue,
        double confidence,
        string? providerName = null,
        string? sourceSpan = null)
        : base(id)
    {
        TenantId = tenantId;
        IngestionJobId = ingestionJobId;
        ContractDocumentVersionId = contractDocumentVersionId;
        FieldName = fieldName;
        SuggestedValue = suggestedValue;
        Confidence = confidence;
        ProviderName = providerName;
        SourceSpan = sourceSpan;
        Status = SuggestionStatus.Pending.ToString();
    }

    public void Accept(string? correctedValue = null)
    {
        Status = SuggestionStatus.Accepted.ToString();
        if (correctedValue != null)
        {
            SuggestedValue = correctedValue;
        }
    }

    public void Reject()
    {
        Status = SuggestionStatus.Rejected.ToString();
    }

    public void MarkAsCorrected(string correctedValue)
    {
        Status = SuggestionStatus.Corrected.ToString();
        SuggestedValue = correctedValue;
    }
}

public enum SuggestionStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Corrected = 3
}
