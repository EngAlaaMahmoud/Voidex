using Domain.Common;
using Domain.Enums;
using System.Collections.Generic;

namespace Domain.Entities;

public class SalesOrder : BaseEntity
{
    public string? Number { get; set; }
    public DateTime? OrderDate { get; set; }
    public SalesOrderStatus? OrderStatus { get; set; }
    public string? Description { get; set; }
    public string? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Tax Withholding (single)
    public string? TaxId { get; set; }
    public Tax? Tax { get; set; }

    // Order-level discount
    public double? Discount { get; set; } = 0;

    // Calculated totals
    public double? BeforeTaxAmount { get; set; }      // Subtotal
    public double? VatAmount { get; set; }            // VAT only
    public double? WithholdingAmount { get; set; }    // Tax Withholding
    public double? AfterTaxAmount { get; set; }       // Final total

    public ICollection<SalesOrderItem> SalesOrderItemList { get; set; } = new List<SalesOrderItem>();
}
