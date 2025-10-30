namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// ETA Error DTO based on official documentation
/// </summary>
public class ETAErrorDto
{
    /// <summary>
    /// Human readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The target/subject of the error
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Document property name that caused the error
    /// </summary>
    public string? PropertyPath { get; set; }

    /// <summary>
    /// List of multiple errors detected
    /// </summary>
    public List<ETAErrorDto>? Details { get; set; }
} 