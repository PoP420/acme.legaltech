using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class GovernmentApprovalTierDto : EntityDto<Guid>
{
    public decimal AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
    public string AuthorityTitle { get; set; } = string.Empty;
    public bool RequiresNedaReview { get; set; }
    public bool RequiresPresident { get; set; }
    public decimal AllowableVariationPercent { get; set; }
}
