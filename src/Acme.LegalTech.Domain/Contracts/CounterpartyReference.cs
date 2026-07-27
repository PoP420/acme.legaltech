using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class CounterpartyReference : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; private set; }
    public string Name { get; private set; }
    public string? ExternalReference { get; private set; }

    public CounterpartyReference() { }

    public CounterpartyReference(Guid id, Guid? tenantId, Guid contractId, string name, string? externalReference = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        Name = name;
        ExternalReference = externalReference;
    }
}
