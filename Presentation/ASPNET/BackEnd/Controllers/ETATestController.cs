using Application.Features.ETAReceiptManager.Commands;
using Application.Features.ETAReceiptManager.Queries;
using Application.Common.DTOs.ETAReceiptSubmission;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.BackEnd.Controllers;


[ApiController]
[Route("api/eta-test")]
[Produces("application/json")]
public class ETATestController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ETATestController> _logger;
    private readonly IConfiguration _configuration;

    public ETATestController(IMediator mediator, ILogger<ETATestController> logger, IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Test ETA authentication with configured or provided credentials
    /// </summary>
    /// <param name="request">Optional authentication request (uses config if not provided)</param>
    /// <returns>Authentication test results</returns>
    [HttpPost("test-authentication")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestAuthentication([FromBody] TestAuthenticationRequest? request = null)
    {
        try
        {
            _logger.LogInformation("Starting ETA authentication test");

            // Use provided request or get from configuration
            var authRequest = new AuthenticateETARequest
            {
                ClientId = request?.ClientId ?? _configuration["ETAConfig:ClientId"] ?? "test-client-id",
                ClientSecret = request?.ClientSecret ?? _configuration["ETAConfig:ClientSecret"] ?? "test-client-secret",
                PosSerial = request?.PosSerial ?? _configuration["ETAConfig:PosSerial"] ?? "Sofy123",
                PresharedKey = request?.PresharedKey ?? _configuration["ETAConfig:PresharedKey"] ?? "TESTKEY",
                PosOsVersion = request?.PosOsVersion ?? "WinPOS",
                PosModelFramework = request?.PosModelFramework ?? "DOTNET"
            };

            var result = await _mediator.Send(authRequest);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("ETA authentication test successful");
                
                return Ok(new 
                { 
                    status = "success", 
                    message = "Authentication test completed successfully!",
                    testType = "authentication",
                    data = new
                    {
                        accessToken = result.Data?.AccessToken?[..Math.Min(20, result.Data.AccessToken.Length)] + "...", // Show only first 20 chars
                        tokenType = result.Data?.TokenType,
                        expiresIn = result.Data?.ExpiresIn,
                        scope = result.Data?.Scope,
                        posSerial = authRequest.PosSerial
                    }
                });
            }

            _logger.LogWarning("ETA authentication test failed: {Error}", result.ErrorMessage);
            
            return BadRequest(new 
            { 
                status = "failed", 
                message = result.ErrorMessage ?? "Authentication test failed",
                testType = "authentication"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA authentication test");
            return BadRequest(new 
            { 
                status = "error", 
                message = ex.Message,
                testType = "authentication"
            });
        }
    }

    /// <summary>
    /// Test receipt submission with sample data
    /// </summary>
    /// <param name="request">Submit receipt test request</param>
    /// <returns>Receipt submission test results</returns>
    [HttpPost("test-submit-receipt")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestSubmitReceipt([FromBody] TestSubmitReceiptRequest request)
    {
        try
        {
            _logger.LogInformation("Starting ETA receipt submission test");

            // First authenticate to get access token
            var authRequest = new AuthenticateETARequest
            {
                ClientId = request.ClientId ?? _configuration["ETAConfig:ClientId"] ?? "test-client-id",
                ClientSecret = request.ClientSecret ?? _configuration["ETAConfig:ClientSecret"] ?? "test-client-secret",
                PosSerial = request.PosSerial ?? _configuration["ETAConfig:PosSerial"] ?? "Sofy123",
                PresharedKey = request.PresharedKey ?? _configuration["ETAConfig:PresharedKey"] ?? "TESTKEY"
            };

            var authResult = await _mediator.Send(authRequest);

            if (!authResult.IsSuccess || authResult.Data?.AccessToken == null)
            {
                return BadRequest(new 
                { 
                    status = "failed", 
                    message = "Authentication failed before submitting receipt",
                    testType = "submit_receipt"
                });
            }

            // Create sample receipt for testing
            var sampleReceipt = new DocumentDto
            {
                ReceiptNumber = $"TEST-{DateTime.Now:yyyyMMddHHmmss}",
                Properties = new Dictionary<string, object>
                {
                    ["dateTimeIssued"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    ["receiptType"] = "R",
                    ["totalAmount"] = 100.00m,
                    ["currency"] = "EGP"
                }
            };

            var submitRequest = new SubmitReceiptsRequest
            {
                Receipts = new List<DocumentDto> { sampleReceipt },
                Signatures = new List<DocumentSignatureDto>(), // Empty for test
                AccessToken = authResult.Data.AccessToken
            };

            var submitResult = await _mediator.Send(submitRequest);

            if (submitResult.IsSuccess)
            {
                _logger.LogInformation("ETA receipt submission test successful");
                
                return Ok(new 
                { 
                    status = "success", 
                    message = "Receipt submission test completed successfully!",
                    testType = "submit_receipt",
                    data = new
                    {
                        submissionUUID = submitResult.Data?.SubmissionUUID,
                        acceptedCount = submitResult.Data?.AcceptedDocuments?.Count ?? 0,
                        rejectedCount = submitResult.Data?.RejectedDocuments?.Count ?? 0,
                        sampleReceiptNumber = sampleReceipt.ReceiptNumber
                    }
                });
            }

            _logger.LogWarning("ETA receipt submission test failed: {Error}", submitResult.ErrorMessage);
            
            return BadRequest(new 
            { 
                status = "failed", 
                message = submitResult.ErrorMessage ?? "Receipt submission test failed",
                testType = "submit_receipt"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA receipt submission test");
            return BadRequest(new 
            { 
                status = "error", 
                message = ex.Message,
                testType = "submit_receipt"
            });
        }
    }

    /// <summary>
    /// Test getting receipt details by UUID
    /// </summary>
    /// <param name="request">Get receipt details test request</param>
    /// <returns>Receipt details test results</returns>
    [HttpPost("test-get-receipt-details")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestGetReceiptDetails([FromBody] TestGetReceiptDetailsRequest request)
    {
        try
        {
            _logger.LogInformation("Starting ETA get receipt details test for UUID: {UUID}", request.ReceiptUuid);

            // First authenticate to get access token
            var authRequest = new AuthenticateETARequest
            {
                ClientId = request.ClientId ?? _configuration["ETAConfig:ClientId"] ?? "test-client-id",
                ClientSecret = request.ClientSecret ?? _configuration["ETAConfig:ClientSecret"] ?? "test-client-secret",
                PosSerial = request.PosSerial ?? _configuration["ETAConfig:PosSerial"] ?? "Sofy123",
                PresharedKey = request.PresharedKey ?? _configuration["ETAConfig:PresharedKey"] ?? "TESTKEY"
            };

            var authResult = await _mediator.Send(authRequest);

            if (!authResult.IsSuccess || authResult.Data?.AccessToken == null)
            {
                return BadRequest(new 
                { 
                    status = "failed", 
                    message = "Authentication failed before getting receipt details",
                    testType = "get_receipt_details"
                });
            }

            var getDetailsRequest = new GetReceiptDetailsRequest
            {
                Uuid = request.ReceiptUuid,
                DateTimeIssued = request.DateTimeIssued,
                AccessToken = authResult.Data.AccessToken
            };

            var result = await _mediator.Send(getDetailsRequest);

            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("ETA get receipt details test successful for UUID: {UUID}", request.ReceiptUuid);
                
                return Ok(new 
                { 
                    status = "success", 
                    message = "Receipt details retrieved successfully!",
                    testType = "get_receipt_details",
                    data = new
                    {
                        uuid = request.ReceiptUuid,
                        submissionUUID = result.Data.SubmissionUUID,
                        dateTimeReceived = result.Data.DateTimeReceived,
                        dateTimeIssued = result.Data.DateTimeIssued,
                        receiptStatus = result.Data.Receipt?.Status,
                        receiptNumber = result.Data.Receipt?.ReceiptNumber
                    }
                });
            }

            _logger.LogWarning("ETA get receipt details test failed for UUID: {UUID}. Error: {Error}", 
                request.ReceiptUuid, result.ErrorMessage);
            
            return BadRequest(new 
            { 
                status = "failed", 
                message = result.ErrorMessage ?? "Receipt details test failed",
                testType = "get_receipt_details"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA get receipt details test");
            return BadRequest(new 
            { 
                status = "error", 
                message = ex.Message,
                testType = "get_receipt_details"
            });
        }
    }

    /// <summary>
    /// Run full workflow test: Authentication -> Submit -> Get Details
    /// </summary>
    /// <param name="request">Full workflow test request</param>
    /// <returns>Complete workflow test results</returns>
    [HttpPost("full-workflow-test")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FullWorkflowTest([FromBody] FullWorkflowTestRequest? request = null)
    {
        try
        {
            _logger.LogInformation("Starting ETA full workflow test");
            
            var results = new Dictionary<string, object>();
            string? accessToken = null;
            string? submissionUuid = null;
            string? receiptUuid = null;

            // Use provided request or defaults from configuration
            var clientId = request?.ClientId ?? _configuration["ETAConfig:ClientId"] ?? "test-client-id";
            var clientSecret = request?.ClientSecret ?? _configuration["ETAConfig:ClientSecret"] ?? "test-client-secret";
            var posSerial = request?.PosSerial ?? _configuration["ETAConfig:PosSerial"] ?? "Sofy123";
            var presharedKey = request?.PresharedKey ?? _configuration["ETAConfig:PresharedKey"] ?? "TESTKEY";

            // Step 1: Test Authentication
            try
            {
                var authResult = await _mediator.Send(new AuthenticateETARequest
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    PosSerial = posSerial,
                    PresharedKey = presharedKey
                });

                accessToken = authResult.Data?.AccessToken;
                results.Add("authentication", new 
                { 
                    success = authResult.IsSuccess,
                    tokenType = authResult.Data?.TokenType,
                    expiresIn = authResult.Data?.ExpiresIn,
                    error = authResult.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                results.Add("authentication", new { success = false, error = ex.Message });
            }

            // Step 2: Test Submit Receipt (if authentication succeeded)
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var sampleReceipt = new DocumentDto
                    {
                        ReceiptNumber = $"WORKFLOW-TEST-{DateTime.Now:yyyyMMddHHmmss}",
                        Properties = new Dictionary<string, object>
                        {
                            ["dateTimeIssued"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            ["receiptType"] = "R",
                            ["totalAmount"] = 150.00m
                        }
                    };

                    var submitResult = await _mediator.Send(new SubmitReceiptsRequest
                    {
                        Receipts = new List<DocumentDto> { sampleReceipt },
                        Signatures = new List<DocumentSignatureDto>(),
                        AccessToken = accessToken
                    });

                    submissionUuid = submitResult.Data?.SubmissionUUID;
                    receiptUuid = submitResult.Data?.AcceptedDocuments?.FirstOrDefault()?.Uuid;

                    results.Add("submitReceipt", new 
                    { 
                        success = submitResult.IsSuccess,
                        submissionUUID = submissionUuid,
                        acceptedCount = submitResult.Data?.AcceptedDocuments?.Count ?? 0,
                        rejectedCount = submitResult.Data?.RejectedDocuments?.Count ?? 0,
                        error = submitResult.ErrorMessage
                    });
                }
                catch (Exception ex)
                {
                    results.Add("submitReceipt", new { success = false, error = ex.Message });
                }
            }
            else
            {
                results.Add("submitReceipt", new { success = false, error = "Skipped due to authentication failure" });
            }

            // Step 3: Test Get Receipt Details (if submission succeeded)
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(receiptUuid))
            {
                try
                {
                    // Wait a moment for the receipt to be processed
                    await Task.Delay(2000);

                    var detailsResult = await _mediator.Send(new GetReceiptDetailsRequest
                    {
                        Uuid = receiptUuid,
                        AccessToken = accessToken
                    });

                    results.Add("getReceiptDetails", new 
                    { 
                        success = detailsResult.IsSuccess,
                        receiptStatus = detailsResult.Data?.Receipt?.Status,
                        dateTimeReceived = detailsResult.Data?.DateTimeReceived,
                        error = detailsResult.ErrorMessage
                    });
                }
                catch (Exception ex)
                {
                    results.Add("getReceiptDetails", new { success = false, error = ex.Message });
                }
            }
            else
            {
                results.Add("getReceiptDetails", new { success = false, error = "Skipped due to previous step failure" });
            }

            // Step 4: Test Combined Authentication and Submit
            try
            {
                var sampleReceipt = new DocumentDto
                {
                    ReceiptNumber = $"COMBINED-TEST-{DateTime.Now:yyyyMMddHHmmss}",
                    Properties = new Dictionary<string, object>
                    {
                        ["dateTimeIssued"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        ["receiptType"] = "R"
                    }
                };

                var combinedResult = await _mediator.Send(new AuthenticateAndSubmitReceiptsRequest
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    PosSerial = posSerial,
                    PresharedKey = presharedKey,
                    Receipts = new List<DocumentDto> { sampleReceipt },
                    Signatures = new List<DocumentSignatureDto>()
                });

                results.Add("authenticateAndSubmit", new 
                { 
                    success = combinedResult.IsSuccess,
                    submissionUUID = combinedResult.SubmissionData?.SubmissionUUID,
                    error = combinedResult.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                results.Add("authenticateAndSubmit", new { success = false, error = ex.Message });
            }

            var overallSuccess = results.Values.All(r => 
            {
                var successProp = r.GetType().GetProperty("success");
                return successProp != null && (bool)successProp.GetValue(r)!;
            });

            _logger.LogInformation("ETA full workflow test completed. Overall success: {Success}", overallSuccess);

            return Ok(new 
            { 
                status = overallSuccess ? "success" : "partial_success", 
                message = "Full workflow test completed",
                testType = "full_workflow",
                overallSuccess = overallSuccess,
                results = results,
                summary = new
                {
                    totalTests = results.Count,
                    passedTests = results.Values.Count(r => 
                    {
                        var successProp = r.GetType().GetProperty("success");
                        return successProp != null && (bool)successProp.GetValue(r)!;
                    }),
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during ETA full workflow test");
            return BadRequest(new 
            { 
                status = "error", 
                message = "Workflow test failed with exception",
                testType = "full_workflow",
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Health check for ETA testing endpoints
    /// </summary>
    /// <returns>Health status and available test endpoints</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            service = "ETA Test Integration",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            availableTestEndpoints = new[]
            {
                "POST /api/eta-test/test-authentication",
                "POST /api/eta-test/test-submit-receipt", 
                "POST /api/eta-test/test-get-receipt-details",
                "POST /api/eta-test/full-workflow-test"
            },
            configuration = new
            {
                clientIdConfigured = !string.IsNullOrEmpty(_configuration["ETAConfig:ClientId"]),
                posSerialConfigured = !string.IsNullOrEmpty(_configuration["ETAConfig:PosSerial"]),
                presharedKeyConfigured = !string.IsNullOrEmpty(_configuration["ETAConfig:PresharedKey"])
            }
        });
    }
}

// Request DTOs for testing
public class TestAuthenticationRequest
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PosSerial { get; set; }
    public string? PosOsVersion { get; set; }
    public string? PosModelFramework { get; set; }
    public string? PresharedKey { get; set; }
}

public class TestSubmitReceiptRequest
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PosSerial { get; set; }
    public string? PresharedKey { get; set; }
}

public class TestGetReceiptDetailsRequest
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PosSerial { get; set; }
    public string? PresharedKey { get; set; }
    public string ReceiptUuid { get; set; } = string.Empty;
    public DateTime? DateTimeIssued { get; set; }
}

public class FullWorkflowTestRequest
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PosSerial { get; set; }
    public string? PresharedKey { get; set; }
}