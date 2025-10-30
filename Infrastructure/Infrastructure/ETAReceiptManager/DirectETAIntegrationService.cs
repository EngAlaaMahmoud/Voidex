using Application.Common.DTOs.ETAAuthentication;
using Application.Common.DTOs.ETAReceiptSubmission;
using Application.Common.DTOs.ETAReceiptDetails;
using Application.Common.Services.ETAReceiptManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ETAReceiptManager;

/// <summary>
/// Direct ETA Integration Service Implementation
/// Based on official ETA eReceipt API documentation
/// </summary>
public class DirectETAIntegrationService : IDirectETAIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DirectETAIntegrationService> _logger;
    private readonly ETAConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public DirectETAIntegrationService(
        HttpClient httpClient,
        ILogger<DirectETAIntegrationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = configuration.GetSection("ETAConfig").Get<ETAConfiguration>() 
            ?? throw new InvalidOperationException("ETAConfig configuration section is missing");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<AuthenticateResponseDto> AuthenticateAsync(
        AuthenticateRequestDto request, 
        AuthenticateHeadersDto headers)
    {
        try
        {
            _logger.LogInformation("Starting ETA POS authentication");

            // Prepare form data as per ETA specification
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", request.GrantType),
                new("client_id", request.ClientId),
                new("client_secret", request.ClientSecret)
            };

            var content = new FormUrlEncodedContent(formData);

            // Create HTTP request with required headers
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _config.IdentityServiceUrl)
            {
                Content = content
            };

            // Add required headers as per ETA documentation
            httpRequest.Headers.Add("posserial", headers.PosSerial);
            httpRequest.Headers.Add("pososversion", headers.PosOsVersion);
            httpRequest.Headers.Add("posmodelframework", headers.PosModelFramework);
            httpRequest.Headers.Add("presharedkey", headers.PresharedKey);

            _logger.LogInformation("Sending authentication request to ETA Identity Service: {Url}", 
                _config.IdentityServiceUrl);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                
                if (tokenResponse != null)
                {
                    var authResponse = new AuthenticateResponseDto
                    {
                        AccessToken = tokenResponse.GetValueOrDefault("access_token")?.ToString() ?? string.Empty,
                        TokenType = tokenResponse.GetValueOrDefault("token_type")?.ToString() ?? string.Empty,
                        ExpiresIn = Convert.ToInt32(tokenResponse.GetValueOrDefault("expires_in")),
                        Scope = tokenResponse.GetValueOrDefault("scope")?.ToString()
                    };

                    _logger.LogInformation("ETA authentication successful. Token expires in {ExpiresIn} seconds", 
                        authResponse.ExpiresIn);

                    return authResponse;
                }
            }

            _logger.LogError("ETA authentication failed. Status: {Status}, Response: {Response}", 
                response.StatusCode, responseContent);

            throw new HttpRequestException($"Authentication failed: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ETA authentication");
            throw;
        }
    }

    public async Task<SubmitReceiptResponseDto> SubmitReceiptAsync(
        SubmitReceiptRequestDto request, 
        string accessToken)
    {
        try
        {
            _logger.LogInformation("Submitting {Count} receipts to ETA", request.Receipts.Count);

            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"{_config.InvoicingServiceBaseUrl}/api/v1/receiptsubmissions")
            {
                Content = content
            };

            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

            _logger.LogInformation("Sending receipt submission request to ETA API");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted) // 202 Accepted
            {
                var submissionResponse = JsonSerializer.Deserialize<SubmitReceiptResponseDto>(
                    responseContent, _jsonOptions);

                _logger.LogInformation("Receipt submission successful. Submission UUID: {SubmissionUUID}", 
                    submissionResponse?.SubmissionUUID);

                return submissionResponse ?? new SubmitReceiptResponseDto();
            }

            _logger.LogError("Receipt submission failed. Status: {Status}, Response: {Response}", 
                response.StatusCode, responseContent);

            throw new HttpRequestException($"Receipt submission failed: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during receipt submission");
            throw;
        }
    }

    public async Task<GetReceiptDetailsResponseDto> GetReceiptDetailsAsync(
        GetReceiptDetailsRequestDto request, 
        string accessToken)
    {
        try
        {
            _logger.LogInformation("Getting receipt details for UUID: {UUID}", request.Uuid);

            // Build URL with optional query parameter
            var url = $"{_config.InvoicingServiceBaseUrl}/api/v1/receipts/{request.Uuid}/details";
            if (request.DateTimeIssued.HasValue)
            {
                url += $"?dateTimeIssued={request.DateTimeIssued.Value:yyyy-MM-ddTHH:mm:ss.fffZ}";
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

            _logger.LogInformation("Sending receipt details request to ETA API");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var receiptDetails = JsonSerializer.Deserialize<GetReceiptDetailsResponseDto>(
                    responseContent, _jsonOptions);

                _logger.LogInformation("Receipt details retrieved successfully for UUID: {UUID}", request.Uuid);

                return receiptDetails ?? new GetReceiptDetailsResponseDto();
            }

            _logger.LogError("Failed to get receipt details. Status: {Status}, Response: {Response}", 
                response.StatusCode, responseContent);

            throw new HttpRequestException($"Failed to get receipt details: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt details for UUID: {UUID}", request.Uuid);
            throw;
        }
    }
} 