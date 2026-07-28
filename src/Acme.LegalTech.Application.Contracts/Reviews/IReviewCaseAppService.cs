using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Reviews;

public interface IReviewCaseAppService : ICrudAppService<
    ReviewCaseDto,
    Guid,
    ReviewCaseGetListInput,
    ReviewCaseCreateDto,
    ReviewCaseUpdateDto>
{
    Task AssignAsync(Guid id, Guid userId);
    Task EscalateAsync(Guid id, string reason, string severity);
    Task CompleteAsync(Guid id);
}