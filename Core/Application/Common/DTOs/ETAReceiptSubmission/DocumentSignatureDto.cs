namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// Document signature DTO based on ETA official documentation
/// </summary>
public class DocumentSignatureDto
{
    /// <summary>
    /// Type of the signature: Issuer (I), ServiceProvider (S)
    /// </summary>
    public string SignatureType { get; set; } = string.Empty;

    /// <summary>
    /// Signature value contains CAdES-BES standard Base64 encoded signature
    /// </summary>
    public string Value { get; set; } = string.Empty;
} 