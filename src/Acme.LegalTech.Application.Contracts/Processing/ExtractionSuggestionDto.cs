using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Processing;

public class ExtractionSuggestionDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid IngestionJobId { get; set; }
    public Guid ContractDocumentVersionId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? SuggestedValue { get; set; }
    public string? OriginalValue { get; set; }
    public double Confidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SourceSpan { get; set; }
    public string? ProviderName { get; set; }
}
