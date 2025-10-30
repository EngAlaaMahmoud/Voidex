using Domain.Common;

namespace Domain.Entities;

public class Tax : BaseEntity
{
    public string? Name { get; set; }
    public double? Percentage { get; set; }
    public string? Description { get; set; }
    public virtual ICollection<SalesOrderTax> SalesOrderTaxes { get; set; } = new List<SalesOrderTax>();
    public virtual ICollection<PurchaseOrderTax> PurchaseOrderTaxes { get; set; } = new List<PurchaseOrderTax>();
    public ICollection<Product>? Products { get; set; } = new List<Product>();



}
