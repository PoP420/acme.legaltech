using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Obligations;

public class ContractObligationCreateDto
{
    public Guid ContractId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? SourceClauseReference { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int Priority { get; set; }
}