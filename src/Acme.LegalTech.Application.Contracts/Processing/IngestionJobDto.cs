using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Processing;

public class IngestionJobDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid ContractDocumentVersionId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
