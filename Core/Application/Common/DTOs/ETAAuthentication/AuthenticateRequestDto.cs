namespace Application.Common.DTOs.ETAAuthentication;

/// <summary>
/// Authentication request DTO based on ETA official documentation
/// POST /connect/token
/// </summary>
public class AuthenticateRequestDto
{
    /// <summary>
    /// Must be "client_credentials"
    /// </summary>
    public string GrantType { get; set; } = "client_credentials";

    /// <summary>
    /// Specific client_id provided by ETA
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Specific client_secret provided by ETA
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
} 