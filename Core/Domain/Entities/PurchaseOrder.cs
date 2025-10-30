using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string? Number { get; set; }
    public DateTime? OrderDate { get; set; }
    public PurchaseOrderStatus? OrderStatus { get; set; }
    public string? Description { get; set; }
    public string? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    //public string? TaxId { get; set; }
    //public Tax? Tax { get; set; }

    // ---- NEW: identical to SalesOrder ----
    public string? TaxId { get; set; }          // Tax Withholding (single)
    public Tax? Tax { get; set; }
    public double? BeforeTaxAmount { get; set; }
    public double? TaxAmount { get; set; }
    public double? AfterTaxAmount { get; set; }

    public double? Discount { get; set; } = 0; // Order-level discount

    // Calculated totals (same names as SalesOrder)
    public double? VatAmount { get; set; }            // VAT only
    public double? WithholdingAmount { get; set; }    // Tax Withholding
    // ---------------------------------------
    public ICollection<PurchaseOrderItem> PurchaseOrderItemList { get; set; } = new List<PurchaseOrderItem>();
    public virtual ICollection<PurchaseOrderTax> PurchaseOrderTaxes { get; set; } = new List<PurchaseOrderTax>();

}
