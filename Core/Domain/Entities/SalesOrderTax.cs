using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SalesOrderTax:BaseEntity
    {
        public string? SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public string? TaxId { get; set; }
        public Tax? Tax { get; set; }
    }
}
