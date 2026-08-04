using System;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;

namespace Acme.LegalTech.Controllers;

[ApiController]
[Route("api/app/contract-document")]
public class ContractDocumentController : LegalTechController
{
    private readonly ContractDocumentAppService _appService;

    public ContractDocumentController(ContractDocumentAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("versions/{contractId:guid}")]
    [Authorize(LegalTechPermissions.Contracts.Default)]
    public async Task<ListResultDto<ContractDocumentVersionDto>> GetVersionsAsync(Guid contractId)
    {
        return await _appService.GetVersionsAsync(contractId);
    }

    [HttpPost("upload/{contractId:guid}")]
    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    [Consumes("multipart/form-data")]
    public async Task<ContractDocumentVersionDto> UploadAsync(Guid contractId, [FromForm] ContractAttachDocumentDto input)
    {
        return await _appService.UploadAsync(contractId, input);
    }

    [HttpGet("{id:guid}")]
    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task<ContractDocumentVersionDto> GetAsync(Guid id)
    {
        return await _appService.GetAsync(id);
    }

    [HttpGet("versions/download/{versionId:guid}")]
    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task<IRemoteStreamContent> DownloadAsync(Guid versionId)
    {
        return await _appService.DownloadAsync(versionId);
    }

    [HttpDelete("versions/{versionId:guid}")]
    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task DeleteVersionAsync(Guid versionId)
    {
        await _appService.DeleteVersionAsync(versionId);
    }
}
