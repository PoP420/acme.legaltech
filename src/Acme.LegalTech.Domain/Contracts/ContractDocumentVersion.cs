using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

public class ContractDocumentVersion : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractId { get; private set; }
    public int VersionNumber { get; private set; }
    public string BlobName { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long FileSize { get; private set; }
    public Guid? UploadedById { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public bool IsLatest { get; private set; }
    public string? ChangeNote { get; private set; }

    public ContractDocumentVersion() { }

    public ContractDocumentVersion(
        Guid id,
        Guid? tenantId,
        Guid contractId,
        int versionNumber,
        string blobName,
        string fileName,
        string contentType,
        long fileSize,
        Guid? uploadedById,
        string? changeNote = null,
        bool? isLatest = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        VersionNumber = versionNumber;
        BlobName = blobName;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        UploadedById = uploadedById;
        UploadedAt = DateTime.Now;
        IsLatest = isLatest ?? true;
        ChangeNote = changeNote;
    }

    public void MarkLatest()
    {
        IsLatest = true;
    }

    public void UnmarkLatest()
    {
        IsLatest = false;
    }
}
