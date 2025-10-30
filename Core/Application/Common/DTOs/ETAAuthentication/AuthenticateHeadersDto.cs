namespace Application.Common.DTOs.ETAAuthentication;

/// <summary>
/// Required headers for ETA authentication based on official documentation
/// </summary>
public class AuthenticateHeadersDto
{
    /// <summary>
    /// POS Serial Number (String 100)
    /// </summary>
    public string PosSerial { get; set; } = string.Empty;

    /// <summary>
    /// POS Version Number (String 50)
    /// </summary>
    public string PosOsVersion { get; set; } = "WinPOS";

    /// <summary>
    /// POS Model Framework Number (String 10)
    /// </summary>
    public string PosModelFramework { get; set; } = "DOTNET";

    /// <summary>
    /// POS Pre shared key (String 200)
    /// </summary>
    public string PresharedKey { get; set; } = string.Empty;
} 