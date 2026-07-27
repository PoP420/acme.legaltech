using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class ContractDocumentVersionDto : EntityDto<Guid>
{
    public Guid ContractId { get; set; }
    public int VersionNumber { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid? UploadedById { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsLatest { get; set; }
    public string? ChangeNote { get; set; }
}
