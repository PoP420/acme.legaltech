using System;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class DocumentExtraction : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractDocumentVersionId { get; protected set; }
    public string ProviderName { get; protected set; } = string.Empty;
    public DateTimeOffset ExtractedAt { get; protected set; }
    public string Status { get; protected set; } = string.Empty;
    public string? ErrorMessage { get; protected set; }

    public string? ExtractedTitle { get; protected set; }
    public string? ExtractedCounterparty { get; protected set; }
    public DateTime? ExtractedEffectiveDate { get; protected set; }
    public DateTime? ExtractedExpirationDate { get; protected set; }
    public string? ExtractedCategory { get; protected set; }
    public string? ExtractedRiskBaseline { get; protected set; }
    public string? ExtractedContractStatus { get; protected set; }

    public string? RawResponse { get; protected set; }

    protected DocumentExtraction() { }

    public DocumentExtraction(
        Guid id,
        Guid? tenantId,
        Guid contractDocumentVersionId,
        string providerName,
        DocumentExtractionResult result)
        : base(id)
    {
        TenantId = tenantId;
        ContractDocumentVersionId = contractDocumentVersionId;
        ProviderName = providerName;
        ExtractedAt = result.ExtractedAt;
        Status = result.IsSuccess ? "Success" : "Failed";
        ErrorMessage = result.ErrorMessage;
        ExtractedTitle = result.ExtractedTitle;
        ExtractedCounterparty = result.ExtractedCounterparty;
        ExtractedEffectiveDate = result.ExtractedEffectiveDate;
        ExtractedExpirationDate = result.ExtractedExpirationDate;
        ExtractedCategory = result.ExtractedCategory;
        ExtractedRiskBaseline = result.ExtractedRiskBaseline;
        ExtractedContractStatus = result.ExtractedStatus;
        RawResponse = result.RawResponse;
    }
}
