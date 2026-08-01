using System;

namespace Acme.LegalTech.Contracts;

public class GovernmentApprovalTierDto
{
    public Guid Id { get; set; }
    public decimal AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
    public string AuthorityTitle { get; set; } = string.Empty;
    public bool RequiresNedaReview { get; set; }
    public bool RequiresPresidentApproval { get; set; }
    public decimal AllowableVariationPercent { get; set; }
}