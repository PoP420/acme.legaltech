using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Obligations;

public class CompletionEvidence : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ObligationId { get; protected set; }
    public ContractObligation Obligation { get; protected set; } = null!;
    public string Title { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string? FileName { get; protected set; }
    public string? BlobName { get; protected set; }
    public DateTime UploadedAt { get; protected set; }
    public Guid? UploadedByUserId { get; protected set; }

    public CompletionEvidence() { }

    public CompletionEvidence(
        Guid id,
        Guid? tenantId,
        Guid obligationId,
        string title,
        string description,
        string? fileName = null,
        string? blobName = null,
        Guid? uploadedByUserId = null)
        : base(id)
    {
        TenantId = tenantId;
        ObligationId = obligationId;
        Title = title;
        Description = description;
        FileName = fileName;
        BlobName = blobName;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
    }
}