using System;
using System.IO;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Acme.LegalTech;

public class ContractDocumentBlobSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<ContractDocumentVersion, Guid> _documentVersionRepository;
    private readonly IBlobContainer<ContractsBlobContainer> _blobContainer;

    public ContractDocumentBlobSeedContributor(
        IRepository<ContractDocumentVersion, Guid> documentVersionRepository,
        IBlobContainer<ContractsBlobContainer> blobContainer)
    {
        _documentVersionRepository = documentVersionRepository;
        _blobContainer = blobContainer;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var versionId = Guid.Parse("1c204b56-e0b7-4834-bbeb-491331286638");
        var existing = await _documentVersionRepository.FirstOrDefaultAsync(v => v.Id == versionId);
        if (existing != null)
        {
            return;
        }

        var contractId = Guid.Parse("8841d12a-87f7-40f2-ac84-ba7bf825a48f");
        var tenantId = context.TenantId;

        var version = new ContractDocumentVersion(
            versionId,
            tenantId,
            contractId,
            1,
            "contracts/software-license-agreement-001/v1.pdf",
            "Software_License_Agreement_v1.pdf",
            "application/pdf",
            245760L,
            null,
            "Initial version submitted for review",
            isLatest: false);

        await _documentVersionRepository.InsertAsync(version);

        var relativePath = Path.Combine("..", "..", "..", "..", "..", "docs", "docfiles", "Software_Licensing_Agreement_SAMPLE.docx");
        var absolutePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));

        if (File.Exists(absolutePath))
        {
            await using var stream = File.OpenRead(absolutePath);
            await _blobContainer.SaveAsync("contracts/software-license-agreement-001/v1.pdf", stream, overrideExisting: true);
        }
    }
}
