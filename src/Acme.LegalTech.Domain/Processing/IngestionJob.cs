using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Processing;

public class IngestionJob : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ContractDocumentVersionId { get; protected set; }
    public string JobType { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = IngestionJobStatus.Pending.ToString();
    public string? ProviderName { get; protected set; }
    public DateTimeOffset? StartedAt { get; protected set; }
    public DateTimeOffset? CompletedAt { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public int RetryCount { get; protected set; }

    public ICollection<ExtractionSuggestion> ExtractionSuggestions { get; protected set; } = new List<ExtractionSuggestion>();
    public ICollection<RiskAssessmentSuggestion> RiskAssessmentSuggestions { get; protected set; } = new List<RiskAssessmentSuggestion>();

    public IngestionJob() { }

    public IngestionJob(
        Guid id,
        Guid? tenantId,
        Guid contractDocumentVersionId,
        string jobType,
        string? providerName = null)
        : base(id)
    {
        TenantId = tenantId;
        ContractDocumentVersionId = contractDocumentVersionId;
        JobType = jobType;
        ProviderName = providerName;
        Status = IngestionJobStatus.Pending.ToString();
        RetryCount = 0;
    }

    public void MarkAsRunning()
    {
        Status = IngestionJobStatus.Running.ToString();
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = IngestionJobStatus.Completed.ToString();
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = IngestionJobStatus.Failed.ToString();
        ErrorMessage = errorMessage;
        CompletedAt = DateTimeOffset.UtcNow;
        RetryCount++;
    }

    public void IncrementRetry()
    {
        RetryCount++;
        Status = IngestionJobStatus.Pending.ToString();
    }
}

public enum IngestionJobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
