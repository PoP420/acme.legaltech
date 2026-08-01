using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

/// <summary>
/// Represents a government approval tier for contracts based on contract value.
/// </summary>
public class GovernmentApprovalTier : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public decimal AmountFrom { get; private set; }
    public decimal? AmountTo { get; private set; } // null means no upper bound
    public string AuthorityTitle { get; private set; } = string.Empty;
    public bool RequiresNedaReview { get; private set; }
    public bool RequiresPresidentApproval { get; private set; }
    public decimal AllowableVariationPercent { get; private set; } // e.g., 5 for 5%

    private GovernmentApprovalTier() { }

    public GovernmentApprovalTier(
        Guid id,
        Guid? tenantId,
        decimal amountFrom,
        decimal? amountTo,
        string authorityTitle,
        bool requiresNedaReview,
        bool requiresPresidentApproval,
        decimal allowableVariationPercent) : base(id)
    {
        TenantId = tenantId;
        AmountFrom = amountFrom;
        AmountTo = amountTo;
        AuthorityTitle = authorityTitle;
        RequiresNedaReview = requiresNedaReview;
        RequiresPresidentApproval = requiresPresidentApproval;
        AllowableVariationPercent = allowableVariationPercent;
    }

    public void UpdateDetails(
        decimal? amountFrom = null,
        decimal? amountTo = null,
        string? authorityTitle = null,
        bool? requiresNedaReview = null,
        bool? requiresPresidentApproval = null,
        decimal? allowableVariationPercent = null)
    {
        if (amountFrom.HasValue)
            AmountFrom = amountFrom.Value;
        if (amountTo.HasValue)
            AmountTo = amountTo.Value;
        if (authorityTitle != null)
            AuthorityTitle = authorityTitle;
        if (requiresNedaReview.HasValue)
            RequiresNedaReview = requiresNedaReview.Value;
        if (requiresPresidentApproval.HasValue)
            RequiresPresidentApproval = requiresPresidentApproval.Value;
        if (allowableVariationPercent.HasValue)
            AllowableVariationPercent = allowableVariationPercent.Value;
    }
}