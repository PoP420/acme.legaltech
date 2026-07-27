using System;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

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

    public Contract() { }

    public Contract(Guid id, string title, string counterpartyName) : base(id)
    {
        Title = title;
        CounterpartyName = counterpartyName;
        Status = ContractStatus.Draft;
    }

    public void UpdateDetails(string title, string counterpartyName)
    {
        Title = title;
        CounterpartyName = counterpartyName;
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
