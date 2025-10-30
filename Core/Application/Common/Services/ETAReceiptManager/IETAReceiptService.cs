using ETA.eReceipt.IntegrationToolkit.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Application.Common.Services.ETAReceiptManager;

public interface IETAReceiptService
{
    Task<InitializeResponseDto> InitializeAsync(InitializeRequestDto request);
    Task<AuthenticateResponseDto> AuthenticateAsync(AuthenticateRequestDto? request);
    Task<RefreshCacheResponseDto> RefreshCacheAsync();
    Task<GenerateUuidResponseDto> GenerateUuidAsync(string receiptJson);
    Task<GenerateQrCodeResponseDto> GenerateQrCodeAsync(string receiptWithUuid);
    Task<IssueReceiptResponseDto> IssueReceiptAsync(string receiptToIssue);
    Task<SubmitReceiptsResponseDto> SubmitReceiptsAsync(SubmitReceiptsRequestDto request);
    Task<SyncSubmissionResponseDto> SyncSubmissionAsync(SyncSubmissionRequestDto request);
    Task<IActionResult> ExportReceiptsAsync(ExportReceiptsRequestDto request);
    Task<SearchReceiptsResponseDto> SearchReceiptsAsync(SearchReceiptsRequestDto request);
    // Task<GetReceiptDetailsResponseDto> GetReceiptDetailsAsync(string receiptId);
    // Task<GetReceiptResponseDto> GetReceiptAsync(string receiptId);
    // Task<GetReceiptDetailsResponseDto> GetReceiptDetailsAnonymouslyAsync(string receiptId);
    // Task<GetReceiptSubmissionResponseDto> GetReceiptSubmissionAsync(string submissionId);
    // Task<GetRecentReceiptsResponseDto> GetRecentReceiptsAsync(GetRecentReceiptsRequestDto request);
    // Task<RequestReceiptPackageResponseDto> RequestReceiptPackageAsync(RequestReceiptPackageRequestDto request);
    // Task<GetPackageRequestsResponseDto> GetPackageRequestsAsync();
   // Task<IActionResult> GetReceiptPackageAsync(string packageId);
} 