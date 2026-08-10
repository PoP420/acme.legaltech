using System;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Acme.LegalTech.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace Acme.LegalTech.Controllers;

[ApiController]
[Route("api/app/ai/ingestion-jobs")]
public class IngestionJobController : LegalTechController
{
    private readonly IIngestionJobAppService _appService;

    public IngestionJobController(IIngestionJobAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("{id:guid}")]
    [Authorize(LegalTechPermissions.AIAssist.Default)]
    public async Task<IngestionJobDto> GetAsync(Guid id)
    {
        return await _appService.GetAsync(id);
    }

    [HttpGet("by-document/{documentVersionId:guid}")]
    [Authorize(LegalTechPermissions.AIAssist.Default)]
    public async Task<ListResultDto<IngestionJobDto>> GetListAsync(Guid documentVersionId)
    {
        return await _appService.GetListAsync(documentVersionId);
    }

    [HttpPost("{contractDocumentVersionId:guid}")]
    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task<IngestionJobDto> CreateAsync(Guid contractDocumentVersionId, [FromBody] CreateIngestionJobInput input)
    {
        return await _appService.CreateAsync(contractDocumentVersionId, input.JobType, input.ProviderName);
    }

    [HttpPost("{id:guid}/run")]
    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task<IngestionJobDto> RunAsync(Guid id)
    {
        return await _appService.RunAsync(id);
    }

    [HttpPost("{id:guid}/retry")]
    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task RetryAsync(Guid id)
    {
        await _appService.RetryAsync(id);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task CancelAsync(Guid id)
    {
        await _appService.CancelAsync(id);
    }
}

public class CreateIngestionJobInput
{
    public string JobType { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
}
