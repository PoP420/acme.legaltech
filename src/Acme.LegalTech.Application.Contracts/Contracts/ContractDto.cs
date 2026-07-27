using System;
using System.Collections.Generic;
using Acme.LegalTech.Common;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class ContractDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public string? DocumentBlobName { get; set; }
    public Guid? TenantId { get; set; }
    public ContractStatus Status { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Category { get; set; }
    public string? RiskBaseline { get; set; }
    public List<ContractTagDto> Tags { get; set; } = new();
    public List<CounterpartyReferenceDto> Counterparties { get; set; } = new();
    public List<ContractDocumentVersionDto> DocumentVersions { get; set; } = new();
}
