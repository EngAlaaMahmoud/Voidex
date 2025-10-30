using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Vat : BaseEntity
    {
        public string? Name { get; set; }
        public double? Percentage { get; set; }
        public string? Description { get; set; }
        public ICollection<Product>? Products { get; set; }


    }
}
