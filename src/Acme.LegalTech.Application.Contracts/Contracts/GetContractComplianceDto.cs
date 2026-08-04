using System;
using System.Collections.Generic;

namespace Acme.LegalTech.Contracts;

public class GetContractComplianceDto
{
    public Guid ContractId { get; set; }
    public string AuthorityTitle { get; set; } = string.Empty;
    public bool RequiresNedaReview { get; set; }
    public bool RequiresPresidentApproval { get; set; }
    public decimal AllowableVariationPercent { get; set; }
    public decimal? ContractValue { get; set; }
    public List<ContractSignatoryDto> Signatories { get; set; } = new();
    public List<VariationOrderDto> VariationOrders { get; set; } = new();
}