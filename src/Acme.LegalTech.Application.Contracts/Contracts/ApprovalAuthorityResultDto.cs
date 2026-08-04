namespace Acme.LegalTech.Contracts;

public class ApprovalAuthorityResultDto
{
    public string AuthorityTitle { get; set; } = string.Empty;
    public bool RequiresNedaReview { get; set; }
    public bool RequiresPresident { get; set; }
    public decimal AllowableVariationPercent { get; set; }
    public string? LastApprovalAuthorityTitle { get; set; }
    public bool LastApprovalRequiresNeda { get; set; }
    public bool LastApprovalRequiresPresident { get; set; }
}
