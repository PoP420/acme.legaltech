using System;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Clauses;

public class ClauseGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
    public Guid? TaxonomyId { get; set; }
    public string? Category { get; set; }
    public string? Jurisdiction { get; set; }
}