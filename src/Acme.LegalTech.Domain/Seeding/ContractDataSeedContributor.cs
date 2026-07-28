using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Common;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Obligations;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Acme.LegalTech;

public class ContractDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<ContractDocumentVersion, Guid> _documentVersionRepository;
    private readonly IRepository<ContractTag, Guid> _tagRepository;
    private readonly IRepository<CounterpartyReference, Guid> _counterpartyReferenceRepository;
    private readonly IRepository<ContractObligation, Guid> _obligationRepository;

    public ContractDataSeedContributor(
        IRepository<Contract, Guid> contractRepository,
        IRepository<ContractDocumentVersion, Guid> documentVersionRepository,
        IRepository<ContractTag, Guid> tagRepository,
        IRepository<CounterpartyReference, Guid> counterpartyReferenceRepository,
        IRepository<ContractObligation, Guid> obligationRepository)
    {
        _contractRepository = contractRepository;
        _documentVersionRepository = documentVersionRepository;
        _tagRepository = tagRepository;
        _counterpartyReferenceRepository = counterpartyReferenceRepository;
        _obligationRepository = obligationRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _contractRepository.GetCountAsync() > 0)
        {
            return;
        }

        var tenantId = context.TenantId;

        var softwareLicense = new Contract(
            Guid.NewGuid(),
            "Software License Agreement - Acme Platform",
            "TechGlobal Corp",
            status: ContractStatus.Active,
            effectiveDate: new DateTime(2025, 01, 01),
            expirationDate: new DateTime(2026, 12, 31),
            category: "Software License",
            riskBaseline: "Medium",
            documentBlobName: "contracts/software-license-agreement-001.pdf")
        {
            OwnerUserId = null
        };

        var ndaGlobal = new Contract(
            Guid.NewGuid(),
            "Global Mutual NDA - Acme & Sunrise Inc",
            "Sunrise Inc",
            status: ContractStatus.Active,
            effectiveDate: new DateTime(2024, 06, 15),
            expirationDate: new DateTime(2026, 06, 15),
            category: "NDA",
            riskBaseline: "Low",
            documentBlobName: "contracts/mutual-nda-sunrise-002.pdf")
        {
            OwnerUserId = null
        };

        var partnershipAgreement = new Contract(
            Guid.NewGuid(),
            "Strategic Partnership Agreement - Logistics",
            "LogiTrans Ltd",
            status: ContractStatus.Draft,
            category: "Partnership",
            riskBaseline: "High")
        {
            OwnerUserId = null
        };

        var vendorSla = new Contract(
            Guid.NewGuid(),
            "Vendor SLA - Cloud Hosting Services",
            "NebulaCloud Ltd",
            status: ContractStatus.Active,
            effectiveDate: new DateTime(2025, 03, 01),
            expirationDate: new DateTime(2027, 02, 28),
            category: "SLA",
            riskBaseline: "Medium",
            documentBlobName: "contracts/vendor-sla-nebulacloud-004.pdf")
        {
            OwnerUserId = null
        };

        var oldConsulting = new Contract(
            Guid.NewGuid(),
            "Consulting Engagement - 2023 Audit",
            "Pinnacle Advisory",
            status: ContractStatus.Expired,
            effectiveDate: new DateTime(2023, 01, 10),
            expirationDate: new DateTime(2024, 01, 09),
            category: "Consulting",
            riskBaseline: "Low",
            documentBlobName: "contracts/consulting-2023-pinnacle-005.pdf")
        {
            OwnerUserId = null
        };

        await _contractRepository.InsertManyAsync(new[]
        {
            softwareLicense, ndaGlobal, partnershipAgreement, vendorSla, oldConsulting
        });

        var softwareLicenseDoc1 = new ContractDocumentVersion(
            Guid.NewGuid(),
            tenantId,
            softwareLicense.Id,
            1,
            "contracts/software-license-agreement-001/v1.pdf",
            "Software_License_Agreement_v1.pdf",
            "application/pdf",
            245760L,
            null,
            "Initial version submitted for review",
            isLatest: false);

        var softwareLicenseDoc2 = new ContractDocumentVersion(
            Guid.NewGuid(),
            tenantId,
            softwareLicense.Id,
            2,
            "contracts/software-license-agreement-001/v2.pdf",
            "Software_License_Agreement_v2.pdf",
            "application/pdf",
            389120L,
            null,
            "Updated pricing and support terms",
            isLatest: true);

        var ndaGlobalDoc1 = new ContractDocumentVersion(
            Guid.NewGuid(),
            tenantId,
            ndaGlobal.Id,
            1,
            "contracts/mutual-nda-sunrise-002/v1.pdf",
            "Mutual_NDA_Sunrise_v1.pdf",
            "application/pdf",
            128000L,
            null,
            "Signed mutual NDA",
            isLatest: true);

        var vendorSlaDoc1 = new ContractDocumentVersion(
            Guid.NewGuid(),
            tenantId,
            vendorSla.Id,
            1,
            "contracts/vendor-sla-nebulacloud-004/v1.pdf",
            "Vendor_SLA_NeulaCloud_v1.pdf",
            "application/pdf",
            512000L,
            null,
            "Initial SLA with uptime guarantees",
            isLatest: true);

        await _documentVersionRepository.InsertManyAsync(new[]
        {
            softwareLicenseDoc1, softwareLicenseDoc2, ndaGlobalDoc1, vendorSlaDoc1
        });

        var tags = new List<ContractTag>
        {
            new ContractTag(Guid.NewGuid(), tenantId, softwareLicense.Id, "finance"),
            new ContractTag(Guid.NewGuid(), tenantId, softwareLicense.Id, "renewal-2026"),
            new ContractTag(Guid.NewGuid(), tenantId, softwareLicense.Id, "enterprise"),
            new ContractTag(Guid.NewGuid(), tenantId, ndaGlobal.Id, "confidential"),
            new ContractTag(Guid.NewGuid(), tenantId, ndaGlobal.Id, "international"),
            new ContractTag(Guid.NewGuid(), tenantId, vendorSla.Id, "critical"),
            new ContractTag(Guid.NewGuid(), tenantId, vendorSla.Id, "infrastructure"),
            new ContractTag(Guid.NewGuid(), tenantId, partnershipAgreement.Id, "pending-review"),
            new ContractTag(Guid.NewGuid(), tenantId, oldConsulting.Id, "archived")
        };

        await _tagRepository.InsertManyAsync(tags);

        var counterpartyRefs = new List<CounterpartyReference>
        {
            new CounterpartyReference(Guid.NewGuid(), tenantId, softwareLicense.Id, "Legal Entity ID", "TECHGLOB-LE-88231"),
            new CounterpartyReference(Guid.NewGuid(), tenantId, softwareLicense.Id, "VAT Number", "TECHGLOB-VAT-EU-44021"),
            new CounterpartyReference(Guid.NewGuid(), tenantId, ndaGlobal.Id, "DUNS Number", "DUNS-88211042"),
            new CounterpartyReference(Guid.NewGuid(), tenantId, vendorSla.Id, "Account ID", "NEBULA-ACT-9923"),
            new CounterpartyReference(Guid.NewGuid(), tenantId, oldConsulting.Id, "Engagement Code", "PINN-2023-AUD-01")
        };

        await _counterpartyReferenceRepository.InsertManyAsync(counterpartyRefs);

        var obligations = new List<ContractObligation>
        {
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                softwareLicense.Id,
                "Submit quarterly utilization report",
                "Submit a detailed quarterly report detailing license seat utilization and user growth metrics.",
                new DateTime(2025, 03, 31),
                "Section 4.2",
                false,
                null,
                2,
                status: ObligationStatus.Pending.ToString()),
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                softwareLicense.Id,
                "Conduct mid-term security audit",
                "Engage an independent auditor to review platform security controls related to TechGlobal data processing.",
                new DateTime(2025, 06, 30),
                "Section 8.1",
                false,
                null,
                3,
                status: ObligationStatus.InProgress.ToString()),
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                vendorSla.Id,
                "Generate monthly uptime SLA report",
                "NebulaCloud to deliver a monthly SLA compliance report with statistics on downtime, incidents, and root-cause analysis.",
                new DateTime(2025, 04, 05),
                null,
                false,
                null,
                1,
                status: ObligationStatus.Pending.ToString()),
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                vendorSla.Id,
                "Review disaster recovery plan",
                "Internal review of NebulaCloud disaster recovery plan and required RTO/RPO targets.",
                new DateTime(2025, 05, 15),
                null,
                false,
                null,
                2,
                status: ObligationStatus.Pending.ToString()),
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                ndaGlobal.Id,
                "Return or destroy confidential materials",
                "Upon contract expiration, return or destroy all confidential documents and notify counterparty of compliance.",
                new DateTime(2026, 06, 15),
                "Section 5",
                false,
                null,
                1,
                status: ObligationStatus.Pending.ToString()),
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                partnershipAgreement.Id,
                "Submit regulatory compliance checklist",
                "Provide completed compliance checklist for partnership agreement approval.",
                new DateTime(2025, 08, 01),
                "Regulatory Requirements",
                false,
                null,
                2,
                status: ObligationStatus.Pending.ToString()),
            new ContractObligation(
                Guid.NewGuid(),
                tenantId,
                oldConsulting.Id,
                "Archive final deliverables",
                "Archive final audit deliverables and sign-off documents.",
                new DateTime(2024, 02, 01),
                null,
                false,
                null,
                1,
                status: ObligationStatus.Completed.ToString())
        };

        await _obligationRepository.InsertManyAsync(obligations);
    }
}
