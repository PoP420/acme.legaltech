using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

/// <summary>
/// Represents a variation order (amendment) to a contract.
/// </summary>
public class VariationOrder : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid OrderId { get; private set; } // Reference to parent contract
    public Guid ContractId { get; private set; } // Denormalized for easier querying
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; } // The delta amount of this variation
    public decimal CumulativeAmount { get; private set; } // Total contract value after this variation
    public string? ApprovedBy { get; private set; } // User who approved this variation
    public DateTime ApprovedOn { get; private set; }

    public VariationOrder() { }

    public VariationOrder(
        Guid id,
        Guid? tenantId,
        Guid orderId,
        Guid contractId,
        string description,
        decimal amount,
        decimal cumulativeAmount,
        string? approvedBy,
        DateTime approvedOn) : base(id)
    {
        TenantId = tenantId;
        OrderId = orderId;
        ContractId = contractId;
        Description = description;
        Amount = amount;
        CumulativeAmount = cumulativeAmount;
        ApprovedBy = approvedBy;
        ApprovedOn = approvedOn;
    }

    public void UpdateDetails(
        string? description = null,
        decimal? amount = null,
        decimal? cumulativeAmount = null,
        string? approvedBy = null,
        DateTime? approvedOn = null)
    {
        if (description != null)
            Description = description;
        if (amount != null)
            Amount = amount.Value;
        if (cumulativeAmount != null)
            CumulativeAmount = cumulativeAmount.Value;
        if (approvedBy != null)
            ApprovedBy = approvedBy;
        if (approvedOn != null)
            ApprovedOn = approvedOn.Value;
    }
}