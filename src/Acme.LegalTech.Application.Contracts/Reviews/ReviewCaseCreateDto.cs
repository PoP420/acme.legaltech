using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ReviewCaseCreateDto
{
    public string Title { get; set; } = string.Empty;
    public Guid ContractId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public int Priority { get; set; }
    public string? Summary { get; set; }
    public DateTime? DueDate { get; set; }
}