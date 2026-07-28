using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Reviews;

public class ReviewComment : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ReviewCaseId { get; protected set; }
    public ReviewCase ReviewCase { get; protected set; } = null!;
    public Guid? AuthorUserId { get; protected set; }
    public string Content { get; protected set; } = string.Empty;

    public ReviewComment() { }

    public ReviewComment(
        Guid id,
        Guid? tenantId,
        Guid reviewCaseId,
        Guid? authorUserId,
        string content)
        : base(id)
    {
        TenantId = tenantId;
        ReviewCaseId = reviewCaseId;
        AuthorUserId = authorUserId;
        Content = content;
    }
}