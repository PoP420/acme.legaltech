using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Processing;

public class SuggestionDecisionDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid SuggestionId { get; set; }
    public string SuggestionType { get; set; } = string.Empty;
    public Guid? DeciderUserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? CorrectedValue { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset DecidedAt { get; set; }
}
