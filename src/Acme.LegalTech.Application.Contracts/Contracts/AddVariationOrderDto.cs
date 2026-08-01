using System;

namespace Acme.LegalTech.Contracts;

public class AddVariationOrderDto
{
    public decimal AmountDelta { get; set; }
    public string? Description { get; set; }
}