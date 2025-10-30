namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// Document DTO for receipt submission - placeholder for actual receipt structure
/// This would contain the full receipt JSON structure as per ETA specification
/// </summary>
public class DocumentDto
{
    /// <summary>
    /// Receipt number/internal ID
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    /// Document UUID
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Additional document properties would be added here based on ETA receipt structure
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
} 