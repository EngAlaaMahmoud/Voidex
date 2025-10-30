using Application.Common.Services.ETAReceiptManager;
using ETA.eReceipt.IntegrationToolkit.Application.Dtos;
using ETA.eReceipt.IntegrationToolkit.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ETAReceiptManager;

public class ETAReceiptService : IETAReceiptService
{
    private readonly ILogger<ETAReceiptService> _logger;
    private readonly IToolkitHandler _toolkitHandler;

    public ETAReceiptService(
        ILogger<ETAReceiptService> logger,
        IToolkitHandler toolkitHandler)
    {
        _logger = logger;
        _toolkitHandler = toolkitHandler;
    }

    public async Task<InitializeResponseDto> InitializeAsync(InitializeRequestDto request)
    {
        _logger.LogInformation("Initializing ETA Receipt Toolkit");
        return await _toolkitHandler.Initialize(request);
    }

    public async Task<AuthenticateResponseDto> AuthenticateAsync(AuthenticateRequestDto? request)
    {
        _logger.LogInformation("Authenticating with ETA");
        return await _toolkitHandler.Authenticate(request);
    }

    public async Task<RefreshCacheResponseDto> RefreshCacheAsync()
    {
        _logger.LogInformation("Refreshing ETA cache");
        return await _toolkitHandler.RefreshCache();
    }

    public async Task<GenerateUuidResponseDto> GenerateUuidAsync(string receiptJson)
    {
        _logger.LogInformation("Generating UUID for receipt");
        return await _toolkitHandler.GenerateUuid(receiptJson);
    }

    public async Task<GenerateQrCodeResponseDto> GenerateQrCodeAsync(string receiptWithUuid)
    {
        _logger.LogInformation("Generating QR code for receipt");
        return await _toolkitHandler.GenerateQrCode(receiptWithUuid);
    }

    public async Task<IssueReceiptResponseDto> IssueReceiptAsync(string receiptToIssue)
    {
        _logger.LogInformation("Issuing receipt");
        return await _toolkitHandler.IssueReceipt(receiptToIssue);
    }

    public async Task<SubmitReceiptsResponseDto> SubmitReceiptsAsync(SubmitReceiptsRequestDto request)
    {
        _logger.LogInformation("Submitting receipts to ETA");
        return await _toolkitHandler.SubmitReceipts(request);
    }

    public async Task<SyncSubmissionResponseDto> SyncSubmissionAsync(SyncSubmissionRequestDto request)
    {
        _logger.LogInformation("Syncing submission with ETA");
        return await _toolkitHandler.SyncSubmission(request);
    }

    public async Task<IActionResult> ExportReceiptsAsync(ExportReceiptsRequestDto request)
    {
        _logger.LogInformation("Exporting receipts");
        return await _toolkitHandler.ExportReceipts(request);
    }

    public async Task<SearchReceiptsResponseDto> SearchReceiptsAsync(SearchReceiptsRequestDto request)
    {
        _logger.LogInformation("Searching receipts");
        return await _toolkitHandler.SearchReceipts(request);
    }
} 