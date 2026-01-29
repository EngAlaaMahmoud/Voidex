using Domain.Common;

namespace Domain.Entities;

public class Tax : BaseEntity
{
    public double? Percentage { get; set; }
    public string? Description { get; set; }
    public string? MainCode { get; set; }
    public string? SubCode { get; set; }
    public string? TaxType { get; set; }

    // ── NEW ──
    public string? TaxCategoryId { get; set; }
    public virtual TaxCategory? TaxCategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<ProductTax> ProductTaxes { get; set; } = new List<ProductTax>();
}
//public class Tax : BaseEntity
//{
//    public double? Percentage { get; set; }
//    public string? Description { get; set; }
//    // New fields for tax register
//    public string? MainCode { get; set; }
//    public string? SubCode { get; set; }
//    public string? TaxType { get; set; }

//    public virtual ICollection<SalesOrderTax> SalesOrderTaxes { get; set; } = new List<SalesOrderTax>();
//    public virtual ICollection<PurchaseOrderTax> PurchaseOrderTaxes { get; set; } = new List<PurchaseOrderTax>();
//    public ICollection<Product>? Products { get; set; } = new List<Product>();
//}
