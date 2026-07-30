using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class DocumentExtractionDto : EntityDto<Guid>
{
    public Guid ContractDocumentVersionId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTimeOffset ExtractedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public string? ExtractedTitle { get; set; }
    public string? ExtractedCounterparty { get; set; }
    public DateTime? ExtractedEffectiveDate { get; set; }
    public DateTime? ExtractedExpirationDate { get; set; }
    public string? ExtractedCategory { get; set; }
    public string? ExtractedRiskBaseline { get; set; }
    public string? ExtractedContractStatus { get; set; }
}
