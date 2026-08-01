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

    // Government contract compliance fields (Task 2)
    public string? DocumentNumber { get; protected set; }
    public string? DocumentSeries { get; protected set; }
    public int? DocumentYear { get; protected set; }
    public DocumentClassification? Classification { get; protected set; }
    public DateTime? RetentionUntil { get; protected set; }
    public decimal? ContractValue { get; protected set; } // Nullable for non-monetary contracts
    public IReadOnlyList<ContractSignatory> Signatories => _signatories.AsReadOnly();
    private readonly List<ContractSignatory> _signatories = new();

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
        // Government contract fields (optional for backward compatibility)
        string? documentNumber = null,
        string? documentSeries = null,
        int? documentYear = null,
        DocumentClassification? classification = null,
        DateTime? retentionUntil = null,
        decimal? contractValue = null) : base(id)
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
        Classification = classification;
        RetentionUntil = retentionUntil;
        ContractValue = contractValue;
    }

    public void UpdateDetails(string title, string counterpartyName)
    {
        Title = title;
        CounterpartyName = counterpartyName;
    }

    /// <summary>
    /// Adds a signatory to the contract.
    /// Throws BusinessException if AuthorizedSignatory role already exists (R2 invariant).
    /// </summary>
    public void AddSignatory(
        GovernmentSignatoryRole role,
        DocumentPartyType partyType,
        string? partyId,
        string? governmentAgency,
        DateTime? signedOn,
        string? capacity,
        int order)
    {
        // R2: Enforce AuthorizedSignatory uniqueness
        if (role == GovernmentSignatoryRole.AuthorizedSignatory &&
            _signatories.Any(s => s.Role == GovernmentSignatoryRole.AuthorizedSignatory))
        {
            throw new BusinessException("LegalTech:Contract:GovSignatoryNotFound")
            {
                Data = { ["Role"] = role.ToString() }
            };
        }

        var signatory = new ContractSignatory(
            Guid.NewGuid(),
            TenantId,
            Id,
            role,
            partyType,
            partyId,
            governmentAgency,
            signedOn,
            capacity,
            order);

        _signatories.Add(signatory);
        // Sort by Order property
        _signatories.Sort((s1, s2) => s1.Order.CompareTo(s2.Order));
    }

    /// <summary>
    /// Removes a signatory from the contract by role.
    /// </summary>
    public void RemoveSignatory(GovernmentSignatoryRole role)
    {
        var signatory = _signatories.FirstOrDefault(s => s.Role == role);
        if (signatory != null)
        {
            _signatories.Remove(signatory);
        }
    }

    /// <summary>
    /// Adds a variation order (amendment) to the contract.
    /// Implements R1: Contract value required for variation.
    /// Implements R3: Variation limit checking against approval tier.
    /// </summary>
    public void AddVariationOrder(decimal amountDelta, GovernmentApprovalTier? approvalTier = null)
    {
        // R1: Guard against null ContractValue for variation orders
        if (ContractValue == null)
        {
            throw new BusinessException("LegalTech:Contract:ValueRequiredForVariation");
        }

        // Store ContractValue in a local variable to avoid CS8602/CS8629 issues
        decimal currentValue = ContractValue.Value;
        var newTotal = currentValue + amountDelta;
        var cumulativePercent = Math.Abs((newTotal - currentValue) / currentValue) * 100;

        // R3: Check against approval tier variation limit (if provided)
        if (approvalTier != null && cumulativePercent > approvalTier.AllowableVariationPercent)
        {
            throw new BusinessException("LegalTech:Contract:ApprovedVariationLimitExceeded")
            {
                Data = {
                    ["ContractValue"] = currentValue,
                    ["NewTotal"] = newTotal,
                    ["CumulativePercent"] = cumulativePercent,
                    ["AllowablePercent"] = approvalTier.AllowableVariationPercent
                }
            };
        }

        // In a full implementation, we would add the variation order to a collection
        // For now, we just validate and update the contract value if within limits
        ContractValue = newTotal;
    }

    /// <summary>
    /// Computes the required approving authority based on contract value.
    /// Returns the matching GovernmentApprovalTier or null if no match found.
    /// </summary>
    public GovernmentApprovalTier? ComputeApprovingAuthority(IEnumerable<GovernmentApprovalTier> tiers)
    {
        if (ContractValue == null)
            return null;

        // Store ContractValue in a local variable to avoid CS8602/CS8629 issues
        decimal currentValue = ContractValue.Value;
        
        return tiers.FirstOrDefault(t =>
            currentValue >= t.AmountFrom &&
            (!t.AmountTo.HasValue || currentValue <= t.AmountTo));
    }

    /// <summary>
    /// Applies an approval to the contract, recording the approving authority.
    /// Note: RequiresPresident/RequiresNedaReview are informational-only in v1 (R8).
    /// </summary>
    public void ApplyApproval(
        GovernmentApprovalTier approvalTier,
        GovernmentSignatoryRole? approvingRole = null,
        string? approvingParty = null,
        DateTime? approvedOn = null)
    {
        // In v1, we record the resolved tier but don't enforce presidential/NEDA requirements
        // These would be enforced in a follow-up slice (R8)

        // For now, we could store the approval tier reference on the contract
        // but since we don't have that field yet, we'll just validate the approach
        // A full implementation would store: ApprovedByTierId, ApprovedByRole, ApprovedByParty, ApprovedOn
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