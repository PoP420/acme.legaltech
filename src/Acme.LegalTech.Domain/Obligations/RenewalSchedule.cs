using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Obligations;

public class RenewalSchedule : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; protected set; }
    public DateTime StartDate { get; protected set; }
    public DateTime EndDate { get; protected set; }
    public DateTime? NextRenewalDate { get; protected set; }
    public int RenewalPeriodDays { get; protected set; }
    public bool AutoRenew { get; protected set; }
    public string Status { get; protected set; } = RenewalStatus.Active.ToString();
    public string? Notes { get; protected set; }

    public RenewalSchedule() { }

    public RenewalSchedule(
        Guid id,
        Guid? tenantId,
        Guid contractId,
        DateTime startDate,
        DateTime endDate,
        DateTime? nextRenewalDate = null,
        int renewalPeriodDays = 365,
        bool autoRenew = false,
        string? notes = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        StartDate = startDate;
        EndDate = endDate;
        NextRenewalDate = nextRenewalDate;
        RenewalPeriodDays = renewalPeriodDays;
        AutoRenew = autoRenew;
        Notes = notes;
        Status = RenewalStatus.Active.ToString();
    }

    public void MarkAsCompleted()
    {
        Status = RenewalStatus.Completed.ToString();
    }

    public void UpdateRenewalDate(DateTime nextRenewalDate)
    {
        NextRenewalDate = nextRenewalDate;
    }
}

public enum RenewalStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2,
    Overdue = 3
}