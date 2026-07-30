using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Acme.LegalTech.Common;
using Acme.LegalTech.Contracts;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Acme.LegalTech.Processing;

public class AzureDocumentIntelligenceExtractionProvider : IDocumentExtractionProvider, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureDocumentIntelligenceExtractionProvider> _logger;

    public AzureDocumentIntelligenceExtractionProvider(
        IConfiguration configuration,
        ILogger<AzureDocumentIntelligenceExtractionProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DocumentExtractionResult> ExtractAsync(
        Volo.Abp.Content.IRemoteStreamContent document,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration["Azure:DocumentIntelligence:Endpoint"];
        var key = _configuration["Azure:DocumentIntelligence:Key"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
        {
            return new DocumentExtractionResult
            {
                IsSuccess = false,
                ErrorMessage = "Azure Document Intelligence is not configured. Set Azure:DocumentIntelligence:Endpoint and Azure:DocumentIntelligence:Key.",
                ProviderName = nameof(AzureDocumentIntelligenceExtractionProvider),
                ExtractedAt = DateTimeOffset.Now
            };
        }

        try
        {
            var credential = new AzureKeyCredential(key);
            var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

            using var stream = document.GetStream();
            var binaryData = BinaryData.FromStream(stream);
            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-contract",
                binaryData,
                cancellationToken: cancellationToken);

            var result = operation.Value;
            var documentResult = result.Documents.Count > 0 ? result.Documents[0] : null;

            var extraction = new DocumentExtractionResult
            {
                IsSuccess = true,
                ProviderName = nameof(AzureDocumentIntelligenceExtractionProvider),
                ExtractedAt = DateTimeOffset.Now,
                RawResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false })
            };

            if (documentResult != null)
            {
                if (documentResult.Fields.TryGetValue("Title", out var titleField) && titleField.Content != null)
                {
                    extraction.ExtractedTitle = titleField.Content.Trim();
                }

                if (documentResult.Fields.TryGetValue("CustomerName", out var customerField) && customerField.Content != null)
                {
                    extraction.ExtractedCounterparty = customerField.Content.Trim();
                }

                if (documentResult.Fields.TryGetValue("EffectiveDate", out var effectiveField) && effectiveField.Content != null)
                {
                    if (DateTime.TryParse(effectiveField.Content, out var effectiveDate))
                    {
                        extraction.ExtractedEffectiveDate = effectiveDate;
                    }
                }

                if (documentResult.Fields.TryGetValue("ExpirationDate", out var expirationField) && expirationField.Content != null)
                {
                    if (DateTime.TryParse(expirationField.Content, out var expirationDate))
                    {
                        extraction.ExtractedExpirationDate = expirationDate;
                    }
                }

                if (documentResult.Fields.TryGetValue("ServiceType", out var categoryField) && categoryField.Content != null)
                {
                    extraction.ExtractedCategory = categoryField.Content.Trim();
                }

                if (documentResult.Fields.TryGetValue("VendorName", out var vendorField) && vendorField.Content != null)
                {
                    if (string.IsNullOrWhiteSpace(extraction.ExtractedCounterparty))
                    {
                        extraction.ExtractedCounterparty = vendorField.Content.Trim();
                    }
                }
            }

            return extraction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document extraction failed for document of type {ContentType}", contentType);

            return new DocumentExtractionResult
            {
                IsSuccess = false,
                ErrorMessage = $"Document extraction failed: {ex.Message}",
                ProviderName = nameof(AzureDocumentIntelligenceExtractionProvider),
                ExtractedAt = DateTimeOffset.Now
            };
        }
    }
}
