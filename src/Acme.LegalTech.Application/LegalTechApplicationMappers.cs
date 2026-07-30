using System;
using Acme.LegalTech.Contracts;

namespace Acme.LegalTech;

public class LegalTechApplicationMappers
{
    public ContractDto MapToContractDto(Contract source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        CounterpartyName = source.CounterpartyName,
        Category = source.Category,
        Status = source.Status,
        EffectiveDate = source.EffectiveDate,
        ExpirationDate = source.ExpirationDate,
        OwnerUserId = source.OwnerUserId,
        RiskBaseline = source.RiskBaseline,
        DocumentBlobName = source.DocumentBlobName
    };

    public ContractTagDto MapToContractTagDto(ContractTag source) => new()
    {
        Id = source.Id,
        ContractId = source.ContractId,
        Name = source.Name
    };

    public CounterpartyReferenceDto MapToCounterpartyReferenceDto(CounterpartyReference source) => new()
    {
        Id = source.Id,
        ContractId = source.ContractId,
        Name = source.Name,
        ExternalReference = source.ExternalReference
    };

    public ContractDocumentVersionDto MapToContractDocumentVersionDto(ContractDocumentVersion source) => new()
    {
        Id = source.Id,
        ContractId = source.ContractId,
        VersionNumber = source.VersionNumber,
        BlobName = source.BlobName,
        FileName = source.FileName,
        ContentType = source.ContentType,
        FileSize = source.FileSize,
        IsLatest = source.IsLatest,
        UploadedById = source.UploadedById,
        ChangeNote = source.ChangeNote,
        UploadedAt = source.UploadedAt
    };

    public DocumentExtractionDto MapToDocumentExtractionDto(DocumentExtraction source) => new()
    {
        Id = source.Id,
        ContractDocumentVersionId = source.ContractDocumentVersionId,
        ProviderName = source.ProviderName,
        ExtractedAt = source.ExtractedAt,
        Status = source.Status,
        ErrorMessage = source.ErrorMessage,
        ExtractedTitle = source.ExtractedTitle,
        ExtractedCounterparty = source.ExtractedCounterparty,
        ExtractedEffectiveDate = source.ExtractedEffectiveDate,
        ExtractedExpirationDate = source.ExtractedExpirationDate,
        ExtractedCategory = source.ExtractedCategory,
        ExtractedRiskBaseline = source.ExtractedRiskBaseline,
        ExtractedContractStatus = source.ExtractedContractStatus
    };

    public Contract MapToContract(ContractCreateDto source)
    {
        return new Contract(Guid.NewGuid(), source.Title, source.CounterpartyName)
        {
            Category = source.Category,
            RiskBaseline = source.RiskBaseline,
            EffectiveDate = source.EffectiveDate,
            ExpirationDate = source.ExpirationDate,
            OwnerUserId = source.OwnerUserId
        };
    }

    public void MapToContract(ContractUpdateDto source, Contract destination)
    {
        destination.UpdateDetails(source.Title, source.CounterpartyName);
        destination.Category = source.Category;
        destination.RiskBaseline = source.RiskBaseline;
        destination.EffectiveDate = source.EffectiveDate;
        destination.ExpirationDate = source.ExpirationDate;
        destination.OwnerUserId = source.OwnerUserId;
    }
}
