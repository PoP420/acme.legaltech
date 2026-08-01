using System;

namespace Acme.LegalTech.Contracts;

public class GetApprovalAuthorityDto
{
    public string AuthorityTitle { get; set; } = string.Empty;
    public bool RequiresNedaReview { get; set; }
    public bool RequiresPresidentApproval { get; set; }
    public decimal AllowableVariationPercent { get; set; }
}