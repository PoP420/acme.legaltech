using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Reviews;
using Acme.LegalTech.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Authorization;

namespace Acme.LegalTech.Reviews;

public class ReviewAppService :
    CrudAppService<
        ReviewCase,
        ReviewCaseDto,
        Guid,
        ReviewCaseGetListInput,
        ReviewCaseCreateDto,
        ReviewCaseUpdateDto>,
    IReviewCaseAppService
{
    private readonly IRepository<ReviewCase, Guid> _repository;
    private readonly IRepository<ReviewTask, Guid> _taskRepository;
    private readonly IRepository<ApprovalStep, Guid> _approvalStepRepository;
    private readonly IRepository<ReviewComment, Guid> _commentRepository;
    private readonly IRepository<EscalationEvent, Guid> _escalationRepository;
    private readonly ICurrentTenant _currentTenant;

    public ReviewAppService(
        IRepository<ReviewCase, Guid> repository,
        IRepository<ReviewTask, Guid> taskRepository,
        IRepository<ApprovalStep, Guid> approvalStepRepository,
        IRepository<ReviewComment, Guid> commentRepository,
        IRepository<EscalationEvent, Guid> escalationRepository,
        ICurrentTenant currentTenant)
        : base(repository)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _approvalStepRepository = approvalStepRepository;
        _commentRepository = commentRepository;
        _escalationRepository = escalationRepository;
        _currentTenant = currentTenant;
    }

    protected override ReviewCase MapToEntity(ReviewCaseCreateDto createInput)
    {
        return new ReviewCase(
            Guid.NewGuid(),
            _currentTenant.Id,
            createInput.Title,
            createInput.ContractId,
            createInput.AssignedUserId,
            createInput.Priority,
            createInput.Summary,
            createInput.DueDate);
    }

    protected override void MapToEntity(ReviewCaseUpdateDto updateInput, ReviewCase entity)
    {
        entity.Update(updateInput.Title, updateInput.AssignedUserId, updateInput.Priority, updateInput.Summary, updateInput.DueDate);
    }

    protected override ReviewCaseDto MapToGetOutputDto(ReviewCase entity)
    {
        var tasks = _taskRepository.GetListAsync(t => t.ReviewCaseId == entity.Id).Result;
        var completedTasks = tasks.Count(t => t.Status == ReviewTaskStatus.Completed.ToString());
        var escalations = _escalationRepository.GetListAsync(e => e.ReviewCaseId == entity.Id).Result;

        return new ReviewCaseDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Title = entity.Title,
            ContractId = entity.ContractId,
            Status = entity.Status,
            AssignedUserId = entity.AssignedUserId,
            DueDate = entity.DueDate,
            Summary = entity.Summary,
            Priority = entity.Priority,
            TaskCount = tasks.Count,
            CompletedTaskCount = completedTasks,
            EscalationCount = escalations.Count,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime
        };
    }

    protected override ReviewCaseDto MapToGetListOutputDto(ReviewCase entity)
    {
        return MapToGetOutputDto(entity);
    }

    protected override async Task<IQueryable<ReviewCase>> CreateFilteredQueryAsync(ReviewCaseGetListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (input.Filter.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c => c.Title.Contains(input.Filter!));
        }

        if (input.Status.IsNullOrWhiteSpace() == false)
        {
            query = query.Where(c => c.Status == input.Status);
        }

        if (input.ContractId.HasValue)
        {
            query = query.Where(c => c.ContractId == input.ContractId.Value);
        }

        if (input.AssignedUserId.HasValue)
        {
            query = query.Where(c => c.AssignedUserId == input.AssignedUserId.Value);
        }

        return query;
    }

    [Authorize(LegalTechPermissions.Reviews.Default)]
    public override async Task<ReviewCaseDto> CreateAsync(ReviewCaseCreateDto input)
    {
        return await base.CreateAsync(input);
    }

    [Authorize(LegalTechPermissions.Reviews.Default)]
    public override async Task<ReviewCaseDto> UpdateAsync(Guid id, ReviewCaseUpdateDto input)
    {
        return await base.UpdateAsync(id, input);
    }

    [Authorize(LegalTechPermissions.Reviews.Assign)]
    public async Task AssignAsync(Guid id, Guid userId)
    {
        var entity = await Repository.GetAsync(id);
        entity.AssignTo(userId);
        await Repository.UpdateAsync(entity);
    }

    [Authorize(LegalTechPermissions.Reviews.Escalate)]
    public async Task EscalateAsync(Guid id, string reason, string severity)
    {
        var entity = await Repository.GetAsync(id);
        var escalation = new EscalationEvent(
            Guid.NewGuid(),
            _currentTenant.Id,
            id,
            reason,
            severity,
            CurrentUser.Id);
        entity.Escalate(escalation);
        await Repository.UpdateAsync(entity);
        await _escalationRepository.InsertAsync(escalation);
    }

    [Authorize(LegalTechPermissions.Reviews.Decide)]
    public async Task CompleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        entity.MarkAsComplete();
        await Repository.UpdateAsync(entity);
    }
}