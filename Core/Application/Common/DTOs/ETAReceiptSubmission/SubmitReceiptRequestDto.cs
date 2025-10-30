namespace Application.Common.DTOs.ETAReceiptSubmission;

/// <summary>
/// Submit receipt request DTO based on ETA official documentation
/// POST /api/v1/receiptsubmissions
/// </summary>
public class SubmitReceiptRequestDto
{
    /// <summary>
    /// List of document objects submitted. List should have at least one document.
    /// </summary>
    public List<DocumentDto> Receipts { get; set; } = new();

    /// <summary>
    /// Structure containing one or two digital signatures
    /// </summary>
    public List<DocumentSignatureDto> Signatures { get; set; } = new();
} 