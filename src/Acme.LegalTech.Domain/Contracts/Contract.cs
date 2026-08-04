using System;
using System.Collections.Generic;
using System.Linq;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

/// <summary>
/// Represents a contract in the legal tech system.
/// </summary>
public class Contract : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string Title { get; protected set; }
    public string CounterpartyName { get; protected set; }
    public string? DocumentBlobName { get; set; }

    public Guid? TenantId { get; protected set; }

    public ContractStatus Status { get; protected set; }

    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Category { get; set; }
    public string? RiskBaseline { get; set; }

    public string? DocumentNumber { get; protected set; }
    public string? DocumentSeries { get; protected set; }
    public int? DocumentYear { get; protected set; }
    public DocumentClassification Classification { get; protected set; }
    public DateTime? RetentionUntil { get; protected set; }
    public decimal? ContractValue { get; protected set; }

    public IReadOnlyCollection<ContractSignatory> Signatories { get; protected set; } = new List<ContractSignatory>();
    public IReadOnlyCollection<VariationOrder> VariationOrders { get; protected set; } = new List<VariationOrder>();

    public string? LastApprovalAuthorityTitle { get; protected set; }
    public bool LastApprovalRequiresNeda { get; protected set; }
    public bool LastApprovalRequiresPresident { get; protected set; }

    public Contract() { }

    public Contract(
        Guid id,
        string title,
        string counterpartyName,
        ContractStatus? status = null,
        DateTime? effectiveDate = null,
        DateTime? expirationDate = null,
        Guid? ownerUserId = null,
        string? category = null,
        string? riskBaseline = null,
        string? documentBlobName = null,
        string? documentNumber = null,
        string? documentSeries = null,
        int? documentYear = null,
        DocumentClassification? classification = null,
        decimal? contractValue = null)
        : base(id)
    {
        Title = title;
        CounterpartyName = counterpartyName;
        Status = status ?? ContractStatus.Draft;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
        OwnerUserId = ownerUserId;
        Category = category;
        RiskBaseline = riskBaseline;
        DocumentBlobName = documentBlobName;
        DocumentNumber = documentNumber;
        DocumentSeries = documentSeries;
        DocumentYear = documentYear;
        Classification = classification ?? DocumentClassification.Unclassified;
        ContractValue = contractValue;
    }

    public void UpdateDetails(string title, string counterpartyName)
    {
        Title = title;
        CounterpartyName = counterpartyName;
    }

    public void SetGovFields(
        string? documentNumber,
        string? documentSeries,
        int? documentYear,
        DocumentClassification? classification,
        decimal? contractValue)
    {
        DocumentNumber = documentNumber;
        DocumentSeries = documentSeries;
        DocumentYear = documentYear;
        if (classification.HasValue)
        {
            Classification = classification.Value;
        }
        ContractValue = contractValue;

        if (EffectiveDate.HasValue)
        {
            RetentionUntil = EffectiveDate.Value.AddYears(5);
        }
    }

    public void AddSignatory(ContractSignatory signatory)
    {
        if (signatory.Role == GovernmentSignatoryRole.AuthorizedSignatory &&
            Signatories.Any(s => s.Role == GovernmentSignatoryRole.AuthorizedSignatory))
        {
            throw new BusinessException("LegalTech:Contract:GovSignatoryExists")
            {
                Data = { ["Role"] = GovernmentSignatoryRole.AuthorizedSignatory.ToString() }
            };
        }

        ((List<ContractSignatory>)Signatories).Add(signatory);
    }

    public void AddVariationOrder(VariationOrder variationOrder, IList<GovernmentApprovalTier> tiers)
    {
        if (ContractValue is null)
        {
            throw new BusinessException("LegalTech:Contract:ValueRequiredForVariation");
        }

        var cumulative = variationOrder.Amount;
        foreach (var vo in VariationOrders)
        {
            cumulative += vo.Amount;
        }

        var tier = ComputeApprovingAuthority(ContractValue.Value, tiers);
        var maxAllowed = ContractValue.Value * (tier.AllowableVariationPercent / 100m);
        if (cumulative > maxAllowed)
        {
            throw new BusinessException("LegalTech:Contract:ApprovedVariationLimitExceeded")
            {
                Data =
                {
                    ["Cumulative"] = cumulative.ToString("F2"),
                    ["MaxAllowed"] = maxAllowed.ToString("F2"),
                    ["AuthorityTitle"] = tier.AuthorityTitle
                }
            };
        }

        variationOrder.CumulativeAmount = cumulative;
        ((List<VariationOrder>)VariationOrders).Add(variationOrder);
    }

    public GovernmentApprovalTier ComputeApprovingAuthority(decimal amount, IList<GovernmentApprovalTier> tiers)
    {
        if (tiers == null || tiers.Count == 0)
        {
            throw new BusinessException("LegalTech:Contract:ApprovalTierNotFound");
        }

        foreach (var tier in tiers.OrderBy(t => t.AmountFrom))
        {
            if (amount >= tier.AmountFrom && (!tier.AmountTo.HasValue || amount <= tier.AmountTo.Value))
            {
                return tier;
            }
        }

        return tiers.OrderByDescending(t => t.AmountFrom).First();
    }

    public void ApplyApproval(GovernmentApprovalTier tier)
    {
        if (tier == null)
        {
            throw new BusinessException("LegalTech:Contract:ApprovalTierNotFound");
        }

        LastApprovalAuthorityTitle = tier.AuthorityTitle;
        LastApprovalRequiresNeda = tier.RequiresNedaReview;
        LastApprovalRequiresPresident = tier.RequiresPresident;
    }

    public void Activate()
    {
        if (Status != ContractStatus.Draft)
        {
            throw new BusinessException("LegalTech:Contract:InvalidStatusTransition")
            {
                Data =
                {
                    ["From"] = Status.ToString(),
                    ["To"] = nameof(ContractStatus.Active)
                }
            };
        }

        Status = ContractStatus.Active;
    }

    public void Expire()
    {
        if (Status != ContractStatus.Active)
        {
            throw new BusinessException("LegalTech:Contract:InvalidStatusTransition")
            {
                Data =
                {
                    ["From"] = Status.ToString(),
                    ["To"] = nameof(ContractStatus.Expired)
                }
            };
        }

        Status = ContractStatus.Expired;
    }

    public void Terminate()
    {
        if (Status is ContractStatus.Expired or ContractStatus.Terminated)
        {
            throw new BusinessException("LegalTech:Contract:InvalidStatusTransition")
            {
                Data =
                {
                    ["From"] = Status.ToString(),
                    ["To"] = nameof(ContractStatus.Terminated)
                }
            };
        }

        Status = ContractStatus.Terminated;
    }
}