using System;
using Volo.Abp.Content;

namespace Acme.LegalTech.Contracts;

public class ContractAttachDocumentDto
{
    public IRemoteStreamContent File { get; set; } = default!;
    public string? ChangeNote { get; set; }
}
