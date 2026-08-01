using System;

namespace Acme.LegalTech.Contracts;

public class ContractSignatoryDto
{
    public Guid Id { get; set; }
    public GovernmentSignatoryRole Role { get; set; }
    public DocumentPartyType PartyType { get; set; }
    public string? PartyId { get; set; }
    public string? GovernmentAgency { get; set; }
    public DateTime? SignedOn { get; set; }
    public string? Capacity { get; set; }
    public int Order { get; set; }
}