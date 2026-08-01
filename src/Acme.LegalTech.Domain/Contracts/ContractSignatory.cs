using System;
using Acme.LegalTech.Common;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

/// <summary>
/// Represents a signatory on a government contract.
/// This is owned by Contract and follows its lifecycle.
/// </summary>
public class ContractSignatory : IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ContractId { get; private set; }
    public GovernmentSignatoryRole Role { get; private set; }
    public DocumentPartyType PartyType { get; private set; }
    public string? PartyId { get; private set; } // System ID or free-text for external
    public string? GovernmentAgency { get; private set; } // Free text / org unit
    public DateTime? SignedOn { get; private set; }
    public string? Capacity { get; private set; } // Free text, e.g. "Head, Procurement Service"
    public int Order { get; private set; } // Display order

    private ContractSignatory() { }

    public ContractSignatory(
        Guid id,
        Guid? tenantId,
        Guid contractId,
        GovernmentSignatoryRole role,
        DocumentPartyType partyType,
        string? partyId,
        string? governmentAgency,
        DateTime? signedOn,
        string? capacity,
        int order)
    {
        // Note: Id parameter is not used as this is an owned entity
        // but kept for constructor pattern consistency
        TenantId = tenantId;
        ContractId = contractId;
        Role = role;
        PartyType = partyType;
        PartyId = partyId;
        GovernmentAgency = governmentAgency;
        SignedOn = signedOn;
        Capacity = capacity;
        Order = order;
    }
}