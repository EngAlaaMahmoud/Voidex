using Domain.Common;

namespace Domain.Entities;

public class Tax : BaseEntity
{
    public double? Percentage { get; set; }
    public string? Description { get; set; }
    // New fields for tax register
    public string? MainCode { get; set; }
    public string? SubCode { get; set; }
    public string? TypeName { get; set; }

    public virtual ICollection<SalesOrderTax> SalesOrderTaxes { get; set; } = new List<SalesOrderTax>();
    public virtual ICollection<PurchaseOrderTax> PurchaseOrderTaxes { get; set; } = new List<PurchaseOrderTax>();
    public ICollection<Product>? Products { get; set; } = new List<Product>();
}
