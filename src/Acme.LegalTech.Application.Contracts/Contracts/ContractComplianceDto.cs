using System;
using Acme.LegalTech.Common;
using System.Collections.Generic;

namespace Acme.LegalTech.Contracts;

public class ContractComplianceDto
{
    public string? DocumentNumber { get; set; }
    public string? DocumentSeries { get; set; }
    public int? DocumentYear { get; set; }
    public DocumentClassification Classification { get; set; }
    public DateTime? RetentionUntil { get; set; }
    public decimal? ContractValue { get; set; }
    public List<ContractSignatoryDto> Signatories { get; set; } = new();
    public List<VariationOrderDto> VariationOrders { get; set; } = new();
    public ApprovalAuthorityResultDto? CurrentAuthority { get; set; }
}
