using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Permissions;
using Acme.LegalTech.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Contracts;

[Authorize(LegalTechPermissions.Contracts.Default)]
public class ContractDocumentAppService : ApplicationService, IContractDocumentAppService
{
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<ContractDocumentVersion, Guid> _documentVersionRepository;
    private readonly IRepository<DocumentExtraction, Guid> _documentExtractionRepository;
    private readonly IBlobContainer<ContractsBlobContainer> _blobContainer;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDocumentExtractionProvider _documentExtractionProvider;

    private static readonly LegalTechApplicationMappers Mappers = new();

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"
    };

    private static readonly Dictionary<string, string> ContentTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".txt"] = "text/plain",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };

    public ContractDocumentAppService(
        IRepository<Contract, Guid> contractRepository,
        IRepository<ContractDocumentVersion, Guid> documentVersionRepository,
        IRepository<DocumentExtraction, Guid> documentExtractionRepository,
        IBlobContainer<ContractsBlobContainer> blobContainer,
        ICurrentTenant currentTenant,
        IDocumentExtractionProvider documentExtractionProvider)
    {
        _contractRepository = contractRepository;
        _documentVersionRepository = documentVersionRepository;
        _documentExtractionRepository = documentExtractionRepository;
        _blobContainer = blobContainer;
        _currentTenant = currentTenant;
        _documentExtractionProvider = documentExtractionProvider;
    }

    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task<ContractDocumentVersionDto> UploadAsync(Guid contractId, ContractAttachDocumentDto input)
    {
        var contract = await _contractRepository.GetAsync(contractId);

        if (input.File.GetStream().Length == 0)
        {
            throw new BusinessException("LegalTech:Contract:UnsupportedFileType")
            {
                Data = { ["Extension"] = string.Empty }
            };
        }

        var extension = Path.GetExtension(input.File.FileName);
        if (extension.IsNullOrWhiteSpace() || !AllowedExtensions.Contains(extension))
        {
            throw new BusinessException("LegalTech:Contract:UnsupportedFileType")
            {
                Data = { ["Extension"] = extension }
            };
        }

        var contentType = ContentTypeMap.GetValueOrDefault(extension, input.File.ContentType ?? "application/octet-stream");

        var blobName = $"contracts/{contractId}/{Guid.NewGuid()}{extension}";

        using var stream = input.File.GetStream();
        var fileSize = stream.Length;
        await _blobContainer.SaveAsync(blobName, stream);

        var versions = await _documentVersionRepository.GetListAsync(v => v.ContractId == contractId);
        var nextVersion = versions.Any() ? versions.Max(v => v.VersionNumber) + 1 : 1;

        foreach (var v in versions.Where(v => v.IsLatest))
        {
            v.UnmarkLatest();
            await _documentVersionRepository.UpdateAsync(v);
        }

        var newVersion = new             ContractDocumentVersion(
            Guid.NewGuid(),
            _currentTenant.Id,
            contractId,
            nextVersion,
            blobName,
            input.File.FileName,
            contentType,
            fileSize,
            CurrentUser.Id,
            input.ChangeNote);

        var saved = await _documentVersionRepository.InsertAsync(newVersion);

        var savedDto = Mappers.MapToContractDocumentVersionDto(saved);

        try
        {
            input.File.GetStream().Position = 0;
            var extractionResult = await _documentExtractionProvider.ExtractAsync(input.File, contentType);
            var extraction = new DocumentExtraction(
                Guid.NewGuid(),
                _currentTenant.Id,
                saved.Id,
                extractionResult.ProviderName ?? "Unknown",
                extractionResult);

            await _documentExtractionRepository.InsertAsync(extraction);

            if (extractionResult.IsSuccess)
            {
                savedDto.ExtractionStatus = "Success";
                savedDto.ExtractedTitle = extractionResult.ExtractedTitle;
                savedDto.ExtractedCounterparty = extractionResult.ExtractedCounterparty;
                savedDto.ExtractedEffectiveDate = extractionResult.ExtractedEffectiveDate;
                savedDto.ExtractedExpirationDate = extractionResult.ExtractedExpirationDate;
                savedDto.ExtractedCategory = extractionResult.ExtractedCategory;
                savedDto.ExtractedRiskBaseline = extractionResult.ExtractedRiskBaseline;
            }
            else
            {
                savedDto.ExtractionStatus = "Failed";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run document extraction for document version {VersionId}", saved.Id);
            savedDto.ExtractionStatus = "Error";
        }

        return savedDto;
    }

    [Authorize(LegalTechPermissions.Contracts.Default)]
    public async Task<ListResultDto<ContractDocumentVersionDto>> GetVersionsAsync(Guid contractId)
    {
        var contract = await _contractRepository.FindAsync(contractId);
        if (contract == null)
        {
            throw new BusinessException("LegalTech:Contract:NotFound")
            {
                Data = { ["ContractId"] = contractId }
            };
        }

        var versions = await _documentVersionRepository.GetListAsync(v => v.ContractId == contractId);
        var sorted = versions.OrderByDescending(v => v.VersionNumber).ToList();
        return new ListResultDto<ContractDocumentVersionDto>(sorted.Select(Mappers.MapToContractDocumentVersionDto).ToList());
    }

    [Authorize(LegalTechPermissions.Contracts.Default)]
    public async Task<IRemoteStreamContent> DownloadAsync(Guid versionId)
    {
        var version = await _documentVersionRepository.GetAsync(versionId);
        var stream = await _blobContainer.GetAsync(version.BlobName);
        return new RemoteStreamContent(stream, version.FileName, version.ContentType);
    }

    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task DeleteVersionAsync(Guid versionId)
    {
        var version = await _documentVersionRepository.GetAsync(versionId);
        await _documentVersionRepository.DeleteAsync(version);
        await _blobContainer.DeleteAsync(version.BlobName);
    }
}
