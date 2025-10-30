namespace Application.Common.DTOs.ETAReceiptDetails;

/// <summary>
/// Get receipt details response DTO based on ETA official documentation
/// </summary>
public class GetReceiptDetailsResponseDto
{
    /// <summary>
    /// Submission UUID
    /// </summary>
    public string? SubmissionUUID { get; set; }

    /// <summary>
    /// Date time when receipt was received by ETA
    /// </summary>
    public DateTime? DateTimeReceived { get; set; }

    /// <summary>
    /// Date time when receipt was issued
    /// </summary>
    public DateTime? DateTimeIssued { get; set; }

    /// <summary>
    /// Submission channel information
    /// </summary>
    public string? SubmissionChannel { get; set; }

    /// <summary>
    /// Maximum precision value
    /// </summary>
    public decimal? MaxPrecision { get; set; }

    /// <summary>
    /// Full receipt details
    /// </summary>
    public ReceiptDetailsDto? Receipt { get; set; }
} 