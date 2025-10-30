namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// Submit receipt response DTO based on ETA official documentation
/// </summary>
public class SubmitReceiptResponseDto
{
    /// <summary>
    /// Unique ID of the submission
    /// </summary>
    public string SubmissionUUID { get; set; } = string.Empty;

    /// <summary>
    /// List of documents that are accepted
    /// </summary>
    public List<DocumentAcceptedDto> AcceptedDocuments { get; set; } = new();

    /// <summary>
    /// List of documents that are not accepted together with their error information
    /// </summary>
    public List<DocumentRejectedDto> RejectedDocuments { get; set; } = new();
} 