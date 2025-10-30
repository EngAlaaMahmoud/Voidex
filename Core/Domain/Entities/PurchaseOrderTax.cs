using Domain.Common;

namespace Domain.Entities
{
    public class PurchaseOrderTax:BaseEntity
    {
        public string? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public string? TaxId { get; set; }
        public Tax? Tax { get; set; }
    }
}
