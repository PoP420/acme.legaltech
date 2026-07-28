using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Playbooks;

public class PlaybookProfileUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}