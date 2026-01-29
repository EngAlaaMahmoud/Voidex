using Domain.Common;

namespace Domain.Entities;

public class Product : BaseEntity
{
    public string? Name { get; set; }
    public string? Number { get; set; }
    public string? Barcode { get; set; } // New field for barcode
    public string? Description { get; set; }
    public double? UnitPrice { get; set; }
    public bool? Physical { get; set; } = true;
    public string? UnitMeasureId { get; set; }
    public UnitMeasure? UnitMeasure { get; set; }
    public string? ProductGroupId { get; set; }
    public ProductGroup? ProductGroup { get; set; }
    public string? ProductCompanyId { get; set; }
    public ProductCompany? ProductCompany { get; set; }
    public string? VatId { get; set; }
    public Vat? Vat { get; set; }
    //public string? TaxId { get; set; }
    //public Tax? Tax { get; set; }
    // Additional fields for extended product table
    public string? InternalCode { get; set; }
    public string? GisEgsCode { get; set; }
    public string? CompanyName { get; set; }
    public string? Model { get; set; }
    public double? Discount { get; set; }
    public double? PriceAfterDiscount { get; set; }
    public double? ServiceFee { get; set; }
    public double? AdditionalTax { get; set; }
    public double? AdditionalFee { get; set; }
    public virtual ICollection<Tax> Taxes { get; set; } = new List<Tax>();
    public virtual ICollection<ProductTax> ProductTaxes { get; set; } = new List<ProductTax>();
}
