using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Obligations;

public class ContractObligationUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
}