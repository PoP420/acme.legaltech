using System;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class ContractSignatory : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; protected set; }
    public GovernmentSignatoryRole Role { get; protected set; }
    public DocumentPartyType PartyType { get; protected set; }
    public string PartyId { get; protected set; } = string.Empty;
    public string GovernmentAgency { get; protected set; } = string.Empty;
    public string Capacity { get; protected set; } = string.Empty;
    public int Order { get; protected set; }
    public DateTime? SignedOn { get; protected set; }

    public ContractSignatory() { }

    public ContractSignatory(
        Guid id,
        Guid? tenantId,
        Guid contractId,
        GovernmentSignatoryRole role,
        DocumentPartyType partyType,
        string partyId,
        string governmentAgency,
        string capacity,
        int order,
        DateTime? signedOn = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        Role = role;
        PartyType = partyType;
        PartyId = Check.NotNullOrWhiteSpace(partyId, nameof(partyId), maxLength: ContractGovConsts.MaxPartyNameLength);
        GovernmentAgency = Check.NotNullOrWhiteSpace(governmentAgency, nameof(governmentAgency));
        Capacity = Check.NotNullOrWhiteSpace(capacity, nameof(capacity), maxLength: ContractGovConsts.MaxCapacityLength);
        Order = order;
        SignedOn = signedOn;
    }
}
