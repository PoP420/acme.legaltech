using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ReviewTaskDto : EntityDto<Guid>
{
    public Guid ReviewCaseId { get; set; }
    public string ReviewCaseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime? DueDate { get; set; }
    public int SortOrder { get; set; }
}