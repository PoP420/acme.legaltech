using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Acme.LegalTech.Processing;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Processing;

[RemoteService(false)]
[Authorize(LegalTechPermissions.AIAssist.Default)]
public class IngestionJobAppService : ApplicationService, IIngestionJobAppService
{
    private readonly IRepository<IngestionJob, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public IngestionJobAppService(
        IRepository<IngestionJob, Guid> repository,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<IngestionJobDto> GetAsync(Guid id)
    {
        var job = await _repository.GetAsync(id);
        return ObjectMapper.Map<IngestionJob, IngestionJobDto>(job);
    }

    public async Task<ListResultDto<IngestionJobDto>> GetListAsync(Guid contractDocumentVersionId)
    {
        var query = await _repository.GetQueryableAsync();
        var jobs = await AsyncExecuter.ToListAsync(
            query.Where(j => j.ContractDocumentVersionId == contractDocumentVersionId));

        return new ListResultDto<IngestionJobDto>(
            ObjectMapper.Map<List<IngestionJob>, List<IngestionJobDto>>(jobs));
    }

    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task<IngestionJobDto> CreateAsync(Guid contractDocumentVersionId, string jobType, string? providerName = null)
    {
        var job = new IngestionJob(
            Guid.NewGuid(),
            _currentTenant.Id,
            contractDocumentVersionId,
            jobType,
            providerName);

        await _repository.InsertAsync(job);
        return ObjectMapper.Map<IngestionJob, IngestionJobDto>(job);
    }

    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task<IngestionJobDto> RunAsync(Guid id)
    {
        var job = await _repository.GetAsync(id);
        job.MarkAsRunning();
        await _repository.UpdateAsync(job);
        return ObjectMapper.Map<IngestionJob, IngestionJobDto>(job);
    }

    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task RetryAsync(Guid id)
    {
        var job = await _repository.GetAsync(id);
        job.IncrementRetry();
        job.MarkAsRunning();
        await _repository.UpdateAsync(job);
    }

    [Authorize(LegalTechPermissions.AIAssist.RunJobs)]
    public async Task CancelAsync(Guid id)
    {
        var job = await _repository.GetAsync(id);
        job.MarkAsFailed("Cancelled by user");
        await _repository.UpdateAsync(job);
    }
}
