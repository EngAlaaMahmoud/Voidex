namespace Application.Common.DTOs.ETAAuthentication;

/// <summary>
/// Authentication response DTO based on ETA official documentation
/// </summary>
public class AuthenticateResponseDto
{
    /// <summary>
    /// Encoded JWT token structure
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Solution returns only Bearer authentication tokens
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// The lifetime of the access token defined in seconds (usually 3600 = 1 hour)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Scope granted to token (e.g., "InvoicingAPI")
    /// </summary>
    public string? Scope { get; set; }
} 