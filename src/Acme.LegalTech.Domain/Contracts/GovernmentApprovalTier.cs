using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class GovernmentApprovalTier : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public decimal AmountFrom { get; protected set; }
    public decimal? AmountTo { get; protected set; }
    public string AuthorityTitle { get; protected set; } = string.Empty;
    public bool RequiresNedaReview { get; protected set; }
    public bool RequiresPresident { get; protected set; }
    public decimal AllowableVariationPercent { get; protected set; }

    public GovernmentApprovalTier() { }

    public GovernmentApprovalTier(
        Guid id,
        Guid? tenantId,
        decimal amountFrom,
        decimal? amountTo,
        string authorityTitle,
        bool requiresNedaReview,
        bool requiresPresident,
        decimal allowableVariationPercent)
        : base(id)
    {
        TenantId = tenantId;
        AmountFrom = amountFrom;
        AmountTo = amountTo;
        AuthorityTitle = Check.NotNullOrWhiteSpace(authorityTitle, nameof(authorityTitle), maxLength: ContractGovConsts.MaxAuthorityTitleLength);
        RequiresNedaReview = requiresNedaReview;
        RequiresPresident = requiresPresident;
        AllowableVariationPercent = allowableVariationPercent;
    }
}
