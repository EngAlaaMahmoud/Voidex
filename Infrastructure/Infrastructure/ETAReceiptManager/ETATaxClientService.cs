using Application.Common.Services.ETAReceiptManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ETAReceiptManager;

public class ETATaxClientService : IETATaxService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ETATaxClientService> _logger;
    private readonly ETAConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public ETATaxClientService(HttpClient httpClient, ILogger<ETATaxClientService> logger, IConfiguration configuration)
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

    public async Task<CustomerTaxInfoDto?> GetCustomerTaxInfoAsync(GetCustomerRequestDto request, string accessToken)
    {
        try
        {
            _logger.LogInformation("Requesting tax info for id: {Id}", request.TaxId);

            // Build endpoint - adjust path if your ETA API differs
            var url = $"{_config.InvoicingServiceBaseUrl}/api/v1/customers/{Uri.EscapeDataString(request.TaxId)}";

            // optional query: include history, details, etc.
            if (!string.IsNullOrEmpty(request.Include))
            {
                url += $"?include={Uri.EscapeDataString(request.Include)}";
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            // if ETA requires POS headers, add them (use values from config)
            if (!string.IsNullOrEmpty(_config.PresharedKey)) httpRequest.Headers.Add("presharedkey", _config.PresharedKey);
            if (!string.IsNullOrEmpty(_config.PosSerial)) httpRequest.Headers.Add("posserial", _config.PosSerial);

            var response = await _httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dto = JsonSerializer.Deserialize<CustomerTaxInfoDto>(content, _jsonOptions);
                _logger.LogInformation("Tax info retrieved for id: {Id}", request.TaxId);
                return dto;
            }

            _logger.LogError("Failed to get tax info. Status: {Status}, Response: {Response}", response.StatusCode, content);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting tax info for id: {Id}", request.TaxId);
            throw;
        }
    }
}

/// <summary>
/// Request DTO for customer lookup
/// </summary>
public class GetCustomerRequestDto
{
    public string TaxId { get; set; } = string.Empty;
    /// <summary>
    /// Optional include param (e.g., &quot;contacts,addresses&quot;)
    /// </summary>
    public string? Include { get; set; }
}

/// <summary>
/// Minimal customer tax info DTO - extend to match ETA response
/// </summary>
public class CustomerTaxInfoDto
{
    public string? TaxId { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
    public DateTime? RegistrationDate { get; set; }
}