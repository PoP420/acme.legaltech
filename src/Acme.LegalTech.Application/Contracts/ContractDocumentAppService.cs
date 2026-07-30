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

    [Authorize(LegalTechPermissions.Contracts.Default)]
    public async Task<ContractDocumentVersionDto> GetAsync(Guid id)
    {
        var version = await _documentVersionRepository.GetAsync(id);
        var dto = Mappers.MapToContractDocumentVersionDto(version);
        
        var extraction = await _documentExtractionRepository.FirstOrDefaultAsync(e => e.ContractDocumentVersionId == id);
        if (extraction != null)
        {
            dto.ExtractionStatus = extraction.Status;
            dto.ExtractedTitle = extraction.ExtractedTitle;
            dto.ExtractedCounterparty = extraction.ExtractedCounterparty;
            dto.ExtractedEffectiveDate = extraction.ExtractedEffectiveDate;
            dto.ExtractedExpirationDate = extraction.ExtractedExpirationDate;
            dto.ExtractedCategory = extraction.ExtractedCategory;
            dto.ExtractedRiskBaseline = extraction.ExtractedRiskBaseline;
        }
        
        return dto;
    }

    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task<ContractDocumentVersionDto> UploadAsync(Guid contractId, ContractAttachDocumentDto input)
    {
        Logger.LogInformation("UploadAsync called for contractId: {ContractId}, FileName: {FileName}, ContentType: {ContentType}, ChangeNote: {ChangeNote}",
            contractId, input.File?.FileName, input.File?.ContentType, input.ChangeNote);

        if (input.File == null)
        {
            Logger.LogError("File is null in upload request");
            throw new BusinessException("LegalTech:Contract:FileRequired");
        }

        var contract = await _contractRepository.FindAsync(contractId);
        if (contract == null)
        {
            Logger.LogError("Contract not found for upload: {ContractId}", contractId);
            throw new BusinessException("LegalTech:Contract:NotFound")
            {
                Data = { ["ContractId"] = contractId.ToString() }
            };
        }

        var stream = input.File.GetStream();
        if (stream == null)
        {
            Logger.LogError("File stream is null in upload request");
            throw new BusinessException("LegalTech:Contract:UnsupportedFileType")
            {
                Data = { ["Extension"] = string.Empty }
            };
        }

        if (stream.Length == 0)
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

        var fileSize = stream.Length;
        
        Logger.LogInformation("Saving blob to container: {BlobName}, FileSize: {FileSize}", blobName, fileSize);
        try
        {
            await _blobContainer.SaveAsync(blobName, stream);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save blob for document version {BlobName}", blobName);
            throw new BusinessException("LegalTech:Contract:UploadFailed")
            {
                Data = { ["FileName"] = input.File.FileName }
            };
        }
        
        Logger.LogInformation("Blob saved successfully: {BlobName}", blobName);

        ContractDocumentVersion newVersion;
        try
        {
            var versions = await _documentVersionRepository.GetListAsync(v => v.ContractId == contractId);
            var nextVersion = versions.Any() ? versions.Max(v => v.VersionNumber) + 1 : 1;

            foreach (var v in versions.Where(v => v.IsLatest))
            {
                v.UnmarkLatest();
                await _documentVersionRepository.UpdateAsync(v);
            }

            newVersion = new ContractDocumentVersion(
                Guid.NewGuid(),
                _currentTenant.Id,
                contractId,
                nextVersion,
                blobName,
                input.File.FileName ?? string.Empty,
                contentType,
                fileSize,
                CurrentUser.Id,
                input.ChangeNote);

            await _documentVersionRepository.InsertAsync(newVersion);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save document version for contract {ContractId}", contractId);
            throw new BusinessException("LegalTech:Contract:UploadFailed")
            {
                Data = { ["ContractId"] = contractId.ToString() }
            };
        }

        var savedDto = Mappers.MapToContractDocumentVersionDto(newVersion);

        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            var extractionResult = await _documentExtractionProvider.ExtractAsync(input.File, contentType);
            var extraction = new DocumentExtraction(
                Guid.NewGuid(),
                _currentTenant.Id,
                newVersion.Id,
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
            Logger.LogError(ex, "Failed to run document extraction for document version {VersionId}", newVersion.Id);
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

        try
        {
            var versions = await _documentVersionRepository.GetListAsync(v => v.ContractId == contractId);
            var sorted = versions.OrderByDescending(v => v.VersionNumber).ToList();
            
            var dtos = new List<ContractDocumentVersionDto>();
            foreach (var version in sorted)
            {
                var dto = Mappers.MapToContractDocumentVersionDto(version);
                
                var extraction = await _documentExtractionRepository.FirstOrDefaultAsync(e => e.ContractDocumentVersionId == version.Id);
                if (extraction != null)
                {
                    dto.ExtractionStatus = extraction.Status;
                    dto.ExtractedTitle = extraction.ExtractedTitle;
                    dto.ExtractedCounterparty = extraction.ExtractedCounterparty;
                    dto.ExtractedEffectiveDate = extraction.ExtractedEffectiveDate;
                    dto.ExtractedExpirationDate = extraction.ExtractedExpirationDate;
                    dto.ExtractedCategory = extraction.ExtractedCategory;
                    dto.ExtractedRiskBaseline = extraction.ExtractedRiskBaseline;
                }
                
                dtos.Add(dto);
            }
            
            return new ListResultDto<ContractDocumentVersionDto>(dtos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get document versions for contract {ContractId}", contractId);
            return new ListResultDto<ContractDocumentVersionDto>(new List<ContractDocumentVersionDto>());
        }
    }

    [Authorize(LegalTechPermissions.Contracts.Default)]
    public async Task<IRemoteStreamContent> DownloadAsync(Guid versionId)
    {
        var version = await _documentVersionRepository.FindAsync(versionId);
        if (version == null)
        {
            throw new BusinessException("LegalTech:Contract:DocumentVersionNotFound")
            {
                Data = { ["VersionId"] = versionId.ToString() }
            };
        }

        try
        {
            var stream = await _blobContainer.GetAsync(version.BlobName);
            return new RemoteStreamContent(stream, version.FileName, version.ContentType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to download document version {VersionId}", versionId);
            throw new BusinessException("LegalTech:Contract:DownloadFailed")
            {
                Data = { ["VersionId"] = versionId.ToString() }
            };
        }
    }

    [Authorize(LegalTechPermissions.Contracts.AttachDocument)]
    public async Task DeleteVersionAsync(Guid versionId)
    {
        var version = await _documentVersionRepository.FindAsync(versionId);
        if (version == null)
        {
            throw new BusinessException("LegalTech:Contract:DocumentVersionNotFound")
            {
                Data = { ["VersionId"] = versionId.ToString() }
            };
        }

        try
        {
            await _documentVersionRepository.DeleteAsync(version);
            await _blobContainer.DeleteAsync(version.BlobName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete document version {VersionId}", versionId);
            throw new BusinessException("LegalTech:Contract:DeleteFailed")
            {
                Data = { ["VersionId"] = versionId.ToString() }
            };
        }
    }
}
