namespace Acme.LegalTech.Contracts;

public class AddVariationOrderDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
