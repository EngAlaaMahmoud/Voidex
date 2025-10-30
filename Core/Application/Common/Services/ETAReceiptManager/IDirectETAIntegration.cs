

using Application.Common.DTOs.ETAAuthentication;
using Application.Common.DTOs.ETAReceiptDetails;
using Application.Common.DTOs.ETAReceiptSubmission;

namespace Application.Common.Services.ETAReceiptManager;

/// <summary>
/// Direct ETA Integration Service Interface
/// Based on official ETA eReceipt API documentation
/// </summary>
public interface IDirectETAIntegration
{
    /// <summary>
    /// Authenticate POS with ETA and get access token
    /// Based on: https://sdk.invoicing.eta.gov.eg/ereceiptapi/01-authenticate-pos/
    /// </summary>
    /// <param name="request">Authentication request</param>
    /// <param name="headers">Required headers for authentication</param>
    /// <returns>Authentication response with access token</returns>
    Task<AuthenticateResponseDto> AuthenticateAsync(
        AuthenticateRequestDto request, 
        AuthenticateHeadersDto headers);

    /// <summary>
    /// Submit receipt documents to ETA
    /// Based on: https://sdk.invoicing.eta.gov.eg/ereceiptapi/02-submit-receipt/
    /// </summary>
    /// <param name="request">Receipt submission request</param>
    /// <param name="accessToken">Bearer token from authentication</param>
    /// <returns>Submission response with accepted/rejected documents</returns>
    Task<SubmitReceiptResponseDto> SubmitReceiptAsync(
        SubmitReceiptRequestDto request, 
        string accessToken);

    /// <summary>
    /// Get receipt details by UUID
    /// Based on: https://sdk.invoicing.eta.gov.eg/ereceiptapi/03-get-receipt-details/
    /// </summary>
    /// <param name="request">Receipt details request</param>
    /// <param name="accessToken">Bearer token from authentication</param>
    /// <returns>Receipt details response</returns>
    Task<GetReceiptDetailsResponseDto> GetReceiptDetailsAsync(
        GetReceiptDetailsRequestDto request, 
        string accessToken);
} 