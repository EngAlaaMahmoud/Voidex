namespace Application.Common.DTOs.ETAReceiptDetails;

/// <summary>
/// Get receipt details request DTO based on ETA official documentation
/// GET /api/v1/receipts/{uuid}/details
/// </summary>
public class GetReceiptDetailsRequestDto
{
    /// <summary>
    /// Unique ID of the receipt to retrieve
    /// </summary>
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Optional: The date and time when the receipt was issued
    /// </summary>
    public DateTime? DateTimeIssued { get; set; }
} 