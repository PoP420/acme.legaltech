using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Contracts;

public class ContractCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Category { get; set; }
    public string? RiskBaseline { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentSeries { get; set; }
    public int? DocumentYear { get; set; }
    public DocumentClassification? Classification { get; set; }
    public DateTime? RetentionUntil { get; set; }
    public decimal? ContractValue { get; set; }
    public List<ContractTagDto> Tags { get; set; } = new();
    public List<CounterpartyReferenceDto> Counterparties { get; set; } = new();
}
