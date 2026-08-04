using System;
using Acme.LegalTech.Common;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class AddSignatoryDto
{
    public GovernmentSignatoryRole Role { get; set; }
    public DocumentPartyType PartyType { get; set; }
    public string PartyId { get; set; } = string.Empty;
    public string GovernmentAgency { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime? SignedOn { get; set; }
}
