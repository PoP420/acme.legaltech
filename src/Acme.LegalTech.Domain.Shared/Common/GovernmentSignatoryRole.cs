using System;

namespace Acme.LegalTech.Common;

/// <summary>
/// Philippine government signatory roles for contracts.
/// </summary>
public enum GovernmentSignatoryRole
{
    PreparedBy = 0,
    ReviewedBy = 1,
    EndorsedBy = 2,
    ApprovedBy = 3,
    AuthorizedSignatory = 4,
    NotedBy = 5
}