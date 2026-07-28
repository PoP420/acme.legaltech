using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Playbooks;

public interface IPlaybookProfileAppService : ICrudAppService<
    PlaybookProfileDto,
    Guid,
    PagedAndSortedResultRequestDto,
    PlaybookProfileCreateDto,
    PlaybookProfileUpdateDto>
{
    Task<PlaybookEvaluationResultDto[]> EvaluateAsync(PlaybookEvaluateInput input);
}