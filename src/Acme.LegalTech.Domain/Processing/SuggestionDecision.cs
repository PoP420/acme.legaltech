using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Processing;

public class SuggestionDecision : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid SuggestionId { get; protected set; }
    public string SuggestionType { get; protected set; } = string.Empty;
    public Guid? DeciderUserId { get; protected set; }
    public string Decision { get; protected set; } = string.Empty;
    public string? CorrectedValue { get; protected set; }
    public string? Comment { get; protected set; }
    public DateTimeOffset DecidedAt { get; protected set; }

    public SuggestionDecision() { }

    public SuggestionDecision(
        Guid id,
        Guid? tenantId,
        Guid suggestionId,
        string suggestionType,
        Guid? deciderUserId,
        string decision,
        string? correctedValue = null,
        string? comment = null)
        : base(id)
    {
        TenantId = tenantId;
        SuggestionId = suggestionId;
        SuggestionType = suggestionType;
        DeciderUserId = deciderUserId;
        Decision = decision;
        CorrectedValue = correctedValue;
        Comment = comment;
        DecidedAt = DateTimeOffset.UtcNow;
    }
}
