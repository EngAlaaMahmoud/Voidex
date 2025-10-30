namespace Infrastructure.ETAReceiptManager;

/// <summary>
/// ETA Configuration based on official documentation
/// </summary>
public class ETAConfiguration
{
    /// <summary>
    /// Client ID provided by ETA
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client Secret provided by ETA
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// POS Serial Number
    /// </summary>
    public string PosSerial { get; set; } = string.Empty;

    /// <summary>
    /// Pre-shared key provided by ETA
    /// </summary>
    public string PresharedKey { get; set; } = string.Empty;

    /// <summary>
    /// ETA Identity Service URL for authentication
    /// </summary>
    public string IdentityServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// ETA Invoicing Service Base URL
    /// </summary>
    public string InvoicingServiceBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// ETA Invoicing Portal Base URL
    /// </summary>
    public string InvoicingPortalBaseUrl { get; set; } = string.Empty;
} 