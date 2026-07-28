using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Clauses;
using Acme.LegalTech.Common;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Obligations;
using Acme.LegalTech.Playbooks;
using Acme.LegalTech.Reviews;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Acme.LegalTech.Data;

public class LegalTechDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<ClauseTemplate, Guid> _clauseRepository;
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<PlaybookProfile, Guid> _playbookRepository;
    private readonly IRepository<PlaybookRule, Guid> _playbookRuleRepository;
    private readonly IRepository<ReviewCase, Guid> _reviewRepository;
    private readonly IRepository<ContractObligation, Guid> _obligationRepository;

    public LegalTechDataSeedContributor(
        IRepository<ClauseTemplate, Guid> clauseRepository,
        IRepository<Contract, Guid> contractRepository,
        IRepository<PlaybookProfile, Guid> playbookRepository,
        IRepository<PlaybookRule, Guid> playbookRuleRepository,
        IRepository<ReviewCase, Guid> reviewRepository,
        IRepository<ContractObligation, Guid> obligationRepository)
    {
        _clauseRepository = clauseRepository;
        _contractRepository = contractRepository;
        _playbookRepository = playbookRepository;
        _playbookRuleRepository = playbookRuleRepository;
        _reviewRepository = reviewRepository;
        _obligationRepository = obligationRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _contractRepository.GetCountAsync() > 0)
        {
            return;
        }

        await SeedClausesAsync();
        await SeedContractsAsync();
        await SeedPlaybooksAsync();
        await SeedReviewsAsync();
        await SeedObligationsAsync();
    }

    private async Task SeedClausesAsync()
    {
        var clauses = new[]
        {
            new ClauseTemplate(Guid.NewGuid(), null, "Non-Disclosure Agreement (NDA)", "Standard NDA clause covering confidentiality obligations, term duration, and permitted disclosures.", null, "US", "Privacy", "confidentiality,nda", "High"),
            new ClauseTemplate(Guid.NewGuid(), null, "Limitation of Liability", "Caps liability at the total fees paid under the agreement and excludes consequential damages.", null, "US", "Liability", "liability,cap", "High"),
            new ClauseTemplate(Guid.NewGuid(), null, "Indemnification", "Each party indemnifies the other against third-party claims arising from breach of representations or warranties.", null, "US", "Liability", "indemnification,claims", "Medium"),
            new ClauseTemplate(Guid.NewGuid(), null, "Governing Law - Delaware", "This agreement shall be governed by and construed in accordance with the laws of the State of Delaware.", null, "US", "Jurisdiction", "governing-law,delaware", "Low"),
            new ClauseTemplate(Guid.NewGuid(), null, "Termination for Convenience", "Either party may terminate this agreement upon 30 days written notice to the other party.", null, "US", "Termination", "termination,convenience", "Medium"),
            new ClauseTemplate(Guid.NewGuid(), null, "Data Processing Agreement (DPA)", "Standard DPA clause for processing personal data in accordance with applicable data protection laws.", null, "EU", "Privacy", "gdpr,dpa,data-protection", "High"),
            new ClauseTemplate(Guid.NewGuid(), null, "Intellectual Property Assignment", "All intellectual property created under this agreement shall be assigned to the Client.", null, "US", "IP", "ip,assignment,ownership", "High"),
            new ClauseTemplate(Guid.NewGuid(), null, "Force Majeure", "Neither party shall be liable for delays or failures in performance resulting from causes beyond its reasonable control.", null, "US", "General", "force-majeure,excuse", "Low"),
        };

        foreach (var clause in clauses)
        {
            await _clauseRepository.InsertAsync(clause);
        }
    }

    private async Task SeedContractsAsync()
    {
        var contracts = new[]
        {
            new Contract(Guid.NewGuid(), "Enterprise SaaS Agreement - Acme Corp", "Acme Corporation"),
            new Contract(Guid.NewGuid(), "Professional Services Agreement - Globex", "Globex Industries"),
            new Contract(Guid.NewGuid(), "Software License Agreement - Initech", "Initech Solutions"),
            new Contract(Guid.NewGuid(), "Consulting Engagement - Umbrella", "Umbrella Holdings"),
            new Contract(Guid.NewGuid(), "Data Processing Agreement - Wayne Ent", "Wayne Enterprises"),
        };

        foreach (var contract in contracts)
        {
            await _contractRepository.InsertAsync(contract);
        }
    }

    private async Task SeedPlaybooksAsync()
    {
        var playbook1 = new PlaybookProfile(Guid.NewGuid(), null, "Standard NDA Review", "Automated review playbook for NDA clauses");
        var playbook2 = new PlaybookProfile(Guid.NewGuid(), null, "Liability Risk Assessment", "Assesses liability clauses for risk exposure");
        var playbook3 = new PlaybookProfile(Guid.NewGuid(), null, "Compliance Check", "Checks clauses for regulatory compliance");

        await _playbookRepository.InsertAsync(playbook1);
        await _playbookRepository.InsertAsync(playbook2);
        await _playbookRepository.InsertAsync(playbook3);

        var rules = new[]
        {
            new PlaybookRule(Guid.NewGuid(), null, playbook1.Id, "NDA Duration Check", "Verifies NDA term is within acceptable range (1-5 years)", "duration>=1 AND duration<=5", RuleSeverity.Medium, "NDA term should be between 1 and 5 years", true, false, false, 1),
            new PlaybookRule(Guid.NewGuid(), null, playbook1.Id, "Permitted Disclosure Check", "Checks if permitted disclosures are properly defined", "permitted-disclosure", RuleSeverity.Low, "Permitted disclosures should be explicitly listed", false, false, false, 2),
            new PlaybookRule(Guid.NewGuid(), null, playbook2.Id, "Liability Cap Check", "Verifies liability cap is present and reasonable", "liability-cap", RuleSeverity.High, "Liability cap is missing or unreasonable", true, false, false, 1),
            new PlaybookRule(Guid.NewGuid(), null, playbook2.Id, "Consequential Damages Exclusion", "Checks if consequential damages are excluded", "consequential-damages", RuleSeverity.Medium, "Consequential damages exclusion is recommended", false, false, false, 2),
            new PlaybookRule(Guid.NewGuid(), null, playbook3.Id, "GDPR Compliance Check", "Verifies GDPR-compliant data processing clauses", "gdpr OR data-protection", RuleSeverity.High, "GDPR compliance clause is required for EU agreements", true, false, false, 1),
            new PlaybookRule(Guid.NewGuid(), null, playbook3.Id, "IP Ownership Check", "Verifies IP ownership assignment is clear", "ip-assignment OR intellectual-property", RuleSeverity.High, "IP ownership assignment must be clearly defined", true, false, false, 2),
        };

        foreach (var rule in rules)
        {
            await _playbookRuleRepository.InsertAsync(rule);
        }
    }

    private async Task SeedReviewsAsync()
    {
        var contracts = await _contractRepository.GetListAsync();
        if (contracts.Count == 0) return;

        var reviews = new[]
        {
            new ReviewCase(Guid.NewGuid(), null, "NDA Review - Acme Corp", contracts[0].Id, null, 2, "Review NDA clause for Acme Corp enterprise agreement", DateTime.UtcNow.AddDays(7)),
            new ReviewCase(Guid.NewGuid(), null, "Liability Assessment - Globex", contracts[1].Id, null, 1, "Assess liability clauses in Globex professional services agreement", DateTime.UtcNow.AddDays(3)),
            new ReviewCase(Guid.NewGuid(), null, "Compliance Check - Wayne Ent", contracts[4].Id, null, 3, "Verify GDPR and data protection compliance for Wayne Enterprises DPA", DateTime.UtcNow.AddDays(5)),
        };

        foreach (var review in reviews)
        {
            await _reviewRepository.InsertAsync(review);
        }
    }

    private async Task SeedObligationsAsync()
    {
        var contracts = await _contractRepository.GetListAsync();
        if (contracts.Count == 0) return;

        var obligations = new[]
        {
            new ContractObligation(Guid.NewGuid(), null, contracts[0].Id, "Deliver final NDA template", "Prepare and deliver the final NDA template for Acme Corp review", DateTime.UtcNow.AddDays(14), null, false, null, 1),
            new ContractObligation(Guid.NewGuid(), null, contracts[1].Id, "Complete liability assessment", "Complete the liability risk assessment for Globex agreement", DateTime.UtcNow.AddDays(5), null, false, null, 2),
            new ContractObligation(Guid.NewGuid(), null, contracts[2].Id, "Update software license terms", "Review and update software license terms for Initech", DateTime.UtcNow.AddDays(21), null, false, null, 1),
            new ContractObligation(Guid.NewGuid(), null, contracts[0].Id, "Schedule compliance review", "Schedule compliance review meeting for Acme Corp agreement", DateTime.UtcNow.AddDays(10), null, true, "monthly", 3),
        };

        foreach (var obligation in obligations)
        {
            await _obligationRepository.InsertAsync(obligation);
        }
    }
}