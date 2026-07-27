using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class ContractTag : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; private set; }
    public string Name { get; private set; }

    public ContractTag() { }

    public ContractTag(Guid id, Guid? tenantId, Guid contractId, string name)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        Name = name;
    }
}
