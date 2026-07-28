using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech.Clauses;

public interface IClauseTemplateAppService : ICrudAppService<
    ClauseTemplateDto,
    Guid,
    ClauseGetListInput,
    ClauseTemplateCreateDto,
    ClauseTemplateUpdateDto>
{
}