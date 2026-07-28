using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ReviewCaseUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public Guid? AssignedUserId { get; set; }
    public int Priority { get; set; }
    public string? Summary { get; set; }
    public DateTime? DueDate { get; set; }
}