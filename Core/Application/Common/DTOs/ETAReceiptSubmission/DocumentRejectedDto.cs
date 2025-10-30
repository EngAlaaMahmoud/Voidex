namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// Document rejected DTO based on ETA official documentation
/// </summary>
public class DocumentRejectedDto
{
    /// <summary>
    /// Internal ID used in submission for the document
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    /// Unique document ID, SHA256 format
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Error information detailing why the document was not accepted
    /// </summary>
    public ETAErrorDto Error { get; set; } = new();
} 