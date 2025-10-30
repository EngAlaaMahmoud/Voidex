using Application.Features.ETAReceiptManager.Commands;
using Application.Features.ETAReceiptManager.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.BackEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ETAReceiptController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ETAReceiptController> _logger;

    public ETAReceiptController(IMediator mediator, ILogger<ETAReceiptController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate with ETA and get access token
    /// Based on: https://sdk.invoicing.eta.gov.eg/ereceiptapi/01-authenticate-pos/
    /// </summary>
    /// <param name="request">Authentication request with credentials</param>
    /// <returns>Access token and authentication details</returns>
    [HttpPost("authenticate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateETARequest request)
    {
        try
        {
            _logger.LogInformation("Processing ETA authentication request for POS Serial: {PosSerial}", request.PosSerial);
            
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("ETA authentication successful for POS Serial: {PosSerial}", request.PosSerial);
                return Ok(new 
                { 
                    success = true, 
                    message = "Authentication successful",
                    data = new
                    {
                        accessToken = result.Data?.AccessToken,
                        tokenType = result.Data?.TokenType,
                        expiresIn = result.Data?.ExpiresIn,
                        scope = result.Data?.Scope
                    }
                });
            }

            _logger.LogWarning("ETA authentication failed for POS Serial: {PosSerial}. Error: {Error}", 
                request.PosSerial, result.ErrorMessage);
                
            return BadRequest(new 
            { 
                success = false, 
                message = result.ErrorMessage ?? "Authentication failed",
                error = "AUTHENTICATION_FAILED"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA authentication for POS Serial: {PosSerial}", request.PosSerial);
            return BadRequest(new 
            { 
                success = false, 
                message = "An error occurred during authentication",
                error = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Submit receipts to ETA for processing
    /// Based on: https://sdk.invoicing.eta.gov.eg/ereceiptapi/02-submit-receipt/
    /// </summary>
    /// <param name="request">Receipt submission request</param>
    /// <returns>Submission results with accepted/rejected receipts</returns>
    [HttpPost("submit-receipts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitReceipts([FromBody] SubmitReceiptsRequest request)
    {
        try
        {
            _logger.LogInformation("Processing ETA receipt submission for {Count} receipts", request.Receipts.Count);
            
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("ETA receipt submission successful. Submission UUID: {SubmissionUUID}", 
                    result.Data?.SubmissionUUID);
                    
                return Ok(new 
                { 
                    success = true, 
                    message = "Receipts submitted successfully",
                    data = new
                    {
                        submissionUUID = result.Data?.SubmissionUUID,
                        acceptedDocuments = result.Data?.AcceptedDocuments,
                        rejectedDocuments = result.Data?.RejectedDocuments,
                        acceptedCount = result.Data?.AcceptedDocuments?.Count ?? 0,
                        rejectedCount = result.Data?.RejectedDocuments?.Count ?? 0
                    }
                });
            }

            _logger.LogWarning("ETA receipt submission failed. Error: {Error}", result.ErrorMessage);
            
            return BadRequest(new 
            { 
                success = false, 
                message = result.ErrorMessage ?? "Receipt submission failed",
                error = "SUBMISSION_FAILED"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA receipt submission");
            return BadRequest(new 
            { 
                success = false, 
                message = "An error occurred during receipt submission",
                error = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Get receipt details by UUID
    /// Based on: https://sdk.invoicing.eta.gov.eg/ereceiptapi/03-get-receipt-details/
    /// </summary>
    /// <param name="uuid">Receipt UUID</param>
    /// <param name="accessToken">Bearer access token</param>
    /// <param name="dateTimeIssued">Optional: Date time when receipt was issued</param>
    /// <returns>Complete receipt details</returns>
    [HttpGet("details/{uuid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptDetails(
        string uuid, 
        [FromQuery] string accessToken,
        [FromQuery] DateTime? dateTimeIssued = null)
    {
        try
        {
            _logger.LogInformation("Getting ETA receipt details for UUID: {UUID}", uuid);
            
            var request = new GetReceiptDetailsRequest 
            { 
                Uuid = uuid, 
                DateTimeIssued = dateTimeIssued,
                AccessToken = accessToken
            };
            
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("ETA receipt details retrieved successfully for UUID: {UUID}", uuid);
                
                return Ok(new 
                { 
                    success = true, 
                    message = "Receipt details retrieved successfully",
                    data = result.Data
                });
            }

            _logger.LogWarning("ETA receipt details not found for UUID: {UUID}. Error: {Error}", uuid, result.ErrorMessage);
            
            return NotFound(new 
            { 
                success = false, 
                message = result.ErrorMessage ?? "Receipt not found",
                error = "RECEIPT_NOT_FOUND"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting ETA receipt details for UUID: {UUID}", uuid);
            return BadRequest(new 
            { 
                success = false, 
                message = "An error occurred while retrieving receipt details",
                error = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Get receipt status by UUID (lightweight version)
    /// </summary>
    /// <param name="uuid">Receipt UUID</param>
    /// <param name="accessToken">Bearer access token</param>
    /// <returns>Receipt status information</returns>
    [HttpGet("status/{uuid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptStatus(string uuid, [FromQuery] string accessToken)
    {
        try
        {
            _logger.LogInformation("Getting ETA receipt status for UUID: {UUID}", uuid);
            
            var request = new GetReceiptStatusRequest 
            { 
                Uuid = uuid, 
                AccessToken = accessToken
            };
            
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("ETA receipt status retrieved successfully for UUID: {UUID}", uuid);
                
                return Ok(new 
                { 
                    success = true, 
                    message = "Receipt status retrieved successfully",
                    data = new
                    {
                        uuid = result.Uuid,
                        status = result.Status,
                        statusReason = result.StatusReason,
                        receiptNumber = result.ReceiptNumber,
                        dateTimeReceived = result.DateTimeReceived,
                        dateTimeIssued = result.DateTimeIssued
                    }
                });
            }

            _logger.LogWarning("ETA receipt status not found for UUID: {UUID}. Error: {Error}", uuid, result.ErrorMessage);
            
            return NotFound(new 
            { 
                success = false, 
                message = result.ErrorMessage ?? "Receipt status not found",
                error = "RECEIPT_NOT_FOUND"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting ETA receipt status for UUID: {UUID}", uuid);
            return BadRequest(new 
            { 
                success = false, 
                message = "An error occurred while retrieving receipt status",
                error = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Synchronize submission status by checking receipt details for multiple UUIDs
    /// </summary>
    /// <param name="request">Sync submission request with UUIDs</param>
    /// <returns>Synchronization results</returns>
    [HttpPost("sync-submission")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncSubmission([FromBody] SyncSubmissionRequest request)
    {
        try
        {
         //   _logger.LogInformation("Syncing ETA submission for {Count} receipts", request.ReceiptUuids.Count);
            
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess)
            {
         //       _logger.LogInformation("ETA submission sync completed. Processed: {Total}, Success: {Success}, Failed: {Failed}", 
            //        result.TotalProcessed, result.SuccessCount, result.FailedCount);
                    
                //return Ok(new 
                //{ 
                //    success = true, 
                //    message = "Submission synchronization completed",
                //    data = new
                //    {
                //        totalProcessed = result.TotalProcessed,
                //        successCount = result.SuccessCount,
                //        failedCount = result.FailedCount,
                //        processedReceipts = result.ProcessedReceipts,
                //        notFoundUuids = result.NotFoundUuids
                //    }
                //});
            }

            _logger.LogWarning("ETA submission sync failed. Error: {Error}", result.ErrorMessage);
            
            return BadRequest(new 
            { 
                success = false, 
                message = result.ErrorMessage ?? "Synchronization failed",
                error = "SYNC_FAILED"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA submission synchronization");
            return BadRequest(new 
            { 
                success = false, 
                message = "An error occurred during synchronization",
                error = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Authenticate and submit receipts in one operation
    /// </summary>
    /// <param name="request">Combined authentication and submission request</param>
    /// <returns>Combined authentication and submission results</returns>
    [HttpPost("authenticate-and-submit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AuthenticateAndSubmitReceipts([FromBody] AuthenticateAndSubmitReceiptsRequest request)
    {
        try
        {
            _logger.LogInformation("Processing ETA authenticate and submit for {Count} receipts", request.Receipts.Count);
            
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("ETA authenticate and submit successful. Submission UUID: {SubmissionUUID}", 
                    result.SubmissionData?.SubmissionUUID);
                    
                return Ok(new 
                { 
                    success = true, 
                    message = "Authentication and receipt submission completed successfully",
                    data = new
                    {
                        accessToken = result.AccessToken,
                        authentication = new
                        {
                            tokenType = result.AuthenticationData?.TokenType,
                            expiresIn = result.AuthenticationData?.ExpiresIn,
                            scope = result.AuthenticationData?.Scope
                        },
                        submission = new
                        {
                            submissionUUID = result.SubmissionData?.SubmissionUUID,
                            acceptedCount = result.SubmissionData?.AcceptedDocuments?.Count ?? 0,
                            rejectedCount = result.SubmissionData?.RejectedDocuments?.Count ?? 0,
                            acceptedDocuments = result.SubmissionData?.AcceptedDocuments,
                            rejectedDocuments = result.SubmissionData?.RejectedDocuments
                        }
                    }
                });
            }

            _logger.LogWarning("ETA authenticate and submit failed. Error: {Error}", result.ErrorMessage);
            
            return BadRequest(new 
            { 
                success = false, 
                message = result.ErrorMessage ?? "Authentication and submission failed",
                error = "OPERATION_FAILED"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA authenticate and submit");
            return BadRequest(new 
            { 
                success = false, 
                message = "An error occurred during authentication and submission",
                error = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Health check endpoint for ETA integration
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            service = "ETA Receipt Integration",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            availableEndpoints = new[]
            {
                "POST /api/ETAReceipt/authenticate",
                "POST /api/ETAReceipt/submit-receipts",
                "GET /api/ETAReceipt/details/{uuid}",
                "GET /api/ETAReceipt/status/{uuid}",
                "POST /api/ETAReceipt/sync-submission",
                "POST /api/ETAReceipt/authenticate-and-submit"
            }
        });
    }
}