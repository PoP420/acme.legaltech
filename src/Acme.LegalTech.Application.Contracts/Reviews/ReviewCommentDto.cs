using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Reviews;

public class ReviewCommentDto : EntityDto<Guid>
{
    public Guid ReviewCaseId { get; set; }
    public string ReviewCaseTitle { get; set; } = string.Empty;
    public Guid? AuthorUserId { get; set; }
    public string? AuthorUserName { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}