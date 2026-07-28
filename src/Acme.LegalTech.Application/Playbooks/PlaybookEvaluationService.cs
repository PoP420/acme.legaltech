using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Clauses;
using Acme.LegalTech.Playbooks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Playbooks;

public class PlaybookEvaluationService : ApplicationService, ITransientDependency
{
    private readonly IRepository<PlaybookProfile, Guid> _playbookRepository;
    private readonly IRepository<PlaybookRule, Guid> _ruleRepository;
    private readonly ICurrentTenant _currentTenant;

    public PlaybookEvaluationService(
        IRepository<PlaybookProfile, Guid> playbookRepository,
        IRepository<PlaybookRule, Guid> ruleRepository,
        ICurrentTenant currentTenant)
    {
        _playbookRepository = playbookRepository;
        _ruleRepository = ruleRepository;
        _currentTenant = currentTenant;
    }

    public async Task<PlaybookEvaluationResultDto[]> EvaluateAsync(PlaybookEvaluateInput input)
    {
        var results = new List<PlaybookEvaluationResultDto>();

        var activePlaybooks = await _playbookRepository.GetListAsync(p => p.IsActive);
        var tenantId = _currentTenant.Id;

        foreach (var playbook in activePlaybooks)
        {
            if (playbook.TenantId.HasValue && playbook.TenantId != tenantId && playbook.TenantId != null)
            {
                continue;
            }

            var rules = await _ruleRepository.GetListAsync(r => r.PlaybookId == playbook.Id);

            foreach (var rule in rules)
            {
                var matched = !string.IsNullOrWhiteSpace(input.ClauseText) &&
                              !string.IsNullOrWhiteSpace(rule.ClausePattern) &&
                              input.ClauseText.Contains(rule.ClausePattern, StringComparison.OrdinalIgnoreCase);

                results.Add(new PlaybookEvaluationResultDto
                {
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    Severity = rule.Severity,
                    Matched = matched,
                    MatchSpan = matched ? rule.ClausePattern : null,
                    Rationale = rule.Rationale,
                    IsPreferred = rule.IsPreferred,
                    IsFallback = rule.IsFallback,
                    IsProhibited = rule.IsProhibited
                });
            }
        }

        return results.ToArray();
    }
}