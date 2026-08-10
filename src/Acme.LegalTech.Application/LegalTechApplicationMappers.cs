using System;
using System.Collections.Generic;
using System.Linq;
using Acme.LegalTech.Common;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Processing;

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
        DocumentBlobName = source.DocumentBlobName,
        DocumentNumber = source.DocumentNumber,
        DocumentSeries = source.DocumentSeries,
        DocumentYear = source.DocumentYear,
        Classification = source.Classification,
        RetentionUntil = source.RetentionUntil,
        ContractValue = source.ContractValue,
        Tags = new List<ContractTagDto>(),
        Counterparties = new List<CounterpartyReferenceDto>(),
        DocumentVersions = new List<ContractDocumentVersionDto>(),
        Signatories = source.Signatories.Select(s => MapToContractSignatoryDto(s)).ToList(),
        VariationOrders = source.VariationOrders.Select(v => MapToVariationOrderDto(v)).ToList()
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

    public ContractSignatoryDto MapToContractSignatoryDto(ContractSignatory source) => new()
    {
        Id = source.Id,
        ContractId = source.ContractId,
        Role = source.Role,
        PartyType = source.PartyType,
        PartyId = source.PartyId,
        GovernmentAgency = source.GovernmentAgency,
        Capacity = source.Capacity,
        Order = source.Order,
        SignedOn = source.SignedOn
    };

    public VariationOrderDto MapToVariationOrderDto(VariationOrder source) => new()
    {
        Id = source.Id,
        ContractId = source.ContractId,
        Description = source.Description,
        Amount = source.Amount,
        CumulativeAmount = source.CumulativeAmount,
        ApprovedBy = source.ApprovedBy,
        ApprovedOn = source.ApprovedOn
    };

    public GovernmentApprovalTierDto MapToGovernmentApprovalTierDto(GovernmentApprovalTier source) => new()
    {
        Id = source.Id,
        AmountFrom = source.AmountFrom,
        AmountTo = source.AmountTo,
        AuthorityTitle = source.AuthorityTitle,
        RequiresNedaReview = source.RequiresNedaReview,
        RequiresPresident = source.RequiresPresident,
        AllowableVariationPercent = source.AllowableVariationPercent
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

    public IngestionJobDto MapToIngestionJobDto(IngestionJob source) => new()
    {
        Id = source.Id,
        TenantId = source.TenantId,
        ContractDocumentVersionId = source.ContractDocumentVersionId,
        JobType = source.JobType,
        Status = source.Status,
        ProviderName = source.ProviderName,
        StartedAt = source.StartedAt,
        CompletedAt = source.CompletedAt,
        ErrorMessage = source.ErrorMessage,
        RetryCount = source.RetryCount
    };

    public ExtractionSuggestionDto MapToExtractionSuggestionDto(ExtractionSuggestion source) => new()
    {
        Id = source.Id,
        TenantId = source.TenantId,
        IngestionJobId = source.IngestionJobId,
        ContractDocumentVersionId = source.ContractDocumentVersionId,
        FieldName = source.FieldName,
        SuggestedValue = source.SuggestedValue,
        OriginalValue = source.OriginalValue,
        Confidence = source.Confidence,
        Status = source.Status,
        SourceSpan = source.SourceSpan,
        ProviderName = source.ProviderName
    };

    public RiskAssessmentSuggestionDto MapToRiskAssessmentSuggestionDto(RiskAssessmentSuggestion source) => new()
    {
        Id = source.Id,
        TenantId = source.TenantId,
        IngestionJobId = source.IngestionJobId,
        ContractId = source.ContractId,
        RiskType = source.RiskType,
        Description = source.Description,
        Severity = source.Severity,
        Confidence = source.Confidence,
        Status = source.Status,
        RuleId = source.RuleId,
        ProviderName = source.ProviderName
    };

    public SuggestionDecisionDto MapToSuggestionDecisionDto(SuggestionDecision source) => new()
    {
        Id = source.Id,
        TenantId = source.TenantId,
        SuggestionId = source.SuggestionId,
        SuggestionType = source.SuggestionType,
        DeciderUserId = source.DeciderUserId,
        Decision = source.Decision,
        CorrectedValue = source.CorrectedValue,
        Comment = source.Comment,
        DecidedAt = source.DecidedAt
    };
}
