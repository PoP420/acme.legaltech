using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Playbooks;

public class PlaybookEvaluateInput
{
    public Guid ContractId { get; set; }
    public string ClauseText { get; set; } = string.Empty;
    public Guid? PlaybookId { get; set; }
}