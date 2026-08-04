using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class VariationOrder : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; protected set; }
    public string Description { get; protected set; } = string.Empty;
    public decimal Amount { get; protected set; }
    public decimal CumulativeAmount { get; set; }
    public Guid? ApprovedBy { get; protected set; }
    public DateTime? ApprovedOn { get; protected set; }

    public VariationOrder() { }

    public VariationOrder(
        Guid id,
        Guid? tenantId,
        Guid contractId,
        string description,
        decimal amount,
        decimal cumulativeAmount,
        Guid? approvedBy = null,
        DateTime? approvedOn = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), maxLength: ContractConsts.MaxChangeNoteLength);
        Amount = amount;
        CumulativeAmount = cumulativeAmount;
        ApprovedBy = approvedBy;
        ApprovedOn = approvedOn;
    }
}
