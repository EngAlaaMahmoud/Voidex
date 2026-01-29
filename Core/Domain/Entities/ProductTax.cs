using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductTax : BaseEntity
    {
        public string ProductId { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;

        public string TaxId { get; set; } = null!;
        public virtual Tax Tax { get; set; } = null!;

        // Optional: if you want ordering or flags per product-tax relation
        public int? DisplayOrder { get; set; }
        public bool? IsDefault { get; set; }

        public double TaxValue { get; set; }  // Add this property if missing

        // Optional: Store calculated values for reference
        public double Percentage { get; set; }
        public string MainCode { get; set; }
        public string SubCode { get; set; }

    }
}
