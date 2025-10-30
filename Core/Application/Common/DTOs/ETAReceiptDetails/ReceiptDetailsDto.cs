namespace Application.Common.DTOs.ETAReceiptDetails;

/// <summary>
/// Receipt details DTO based on ETA official documentation
/// </summary>
public class ReceiptDetailsDto
{
    public string? Uuid { get; set; }
    public string? LongId { get; set; }
    public string? PreviousUUID { get; set; }
    public string? ReferenceOldUUID { get; set; }
    public string? ReferenceUUID { get; set; }
    public DateTime? DateTimeIssued { get; set; }
    public DateTime? DateTimeReceived { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Currency { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? SOrderNameCode { get; set; }
    public string? OrderDeliveryMode { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string? Status { get; set; }
    public string? StatusReason { get; set; }
    public bool? HasReturnReceipts { get; set; }
    public List<ReceiptHistoryDto>? History { get; set; }
} 