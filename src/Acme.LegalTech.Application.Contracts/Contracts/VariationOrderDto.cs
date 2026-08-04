using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class VariationOrderDto : EntityDto<Guid>
{
    public Guid ContractId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal CumulativeAmount { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }
}
