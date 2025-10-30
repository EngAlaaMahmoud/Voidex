namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// Document accepted DTO based on ETA official documentation
/// </summary>
public class DocumentAcceptedDto
{
    /// <summary>
    /// Unique document ID, SHA256 format
    /// </summary>
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Unique long Id assigned by eInvoicing and eReceipt solution
    /// </summary>
    public string LongId { get; set; } = string.Empty;

    /// <summary>
    /// Internal ID used in submission for the document
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;
} 