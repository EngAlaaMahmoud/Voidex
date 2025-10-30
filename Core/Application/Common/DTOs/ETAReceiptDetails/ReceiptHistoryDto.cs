namespace Application.Common.DTOs.ETAReceiptDetails;

/// <summary>
/// Receipt history DTO based on ETA official documentation
/// </summary>
public class ReceiptHistoryDto
{
    /// <summary>
    /// Date time of the receipt in history
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Status of the receipt in history
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status reason of the receipt in history
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Submission Uuid of the receipt in history
    /// </summary>
    public string? SubmissionUuid { get; set; }

    /// <summary>
    /// Cancelled by of the receipt in history
    /// </summary>
    public string? CanceledBy { get; set; }
} 