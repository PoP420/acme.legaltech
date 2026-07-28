using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Playbooks;

public class PlaybookProfileDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public List<PlaybookRuleDto> Rules { get; set; } = new();
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
}