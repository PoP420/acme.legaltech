using System;
using System.Collections.Generic;

namespace Acme.LegalTech.Common;

public class DocumentExtractionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProviderName { get; set; }
    public DateTimeOffset ExtractedAt { get; set; }

    public string? ExtractedTitle { get; set; }
    public string? ExtractedCounterparty { get; set; }
    public DateTime? ExtractedEffectiveDate { get; set; }
    public DateTime? ExtractedExpirationDate { get; set; }
    public string? ExtractedCategory { get; set; }
    public string? ExtractedRiskBaseline { get; set; }
    public string? ExtractedStatus { get; set; }

    public List<DocumentExtractedObligation> Obligations { get; set; } = new();
    public List<string> DetectedTags { get; set; } = new();

    public string? RawResponse { get; set; }
}
