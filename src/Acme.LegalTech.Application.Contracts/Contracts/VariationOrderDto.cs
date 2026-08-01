using System;

namespace Acme.LegalTech.Contracts;

public class VariationOrderDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal CumulativeAmount { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime ApprovedOn { get; set; }
}