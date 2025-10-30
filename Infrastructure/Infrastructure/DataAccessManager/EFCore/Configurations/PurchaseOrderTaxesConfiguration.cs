using Domain.Entities;
using Infrastructure.DataAccessManager.EFCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccessManager.EFCore.Configurations
{
    public class PurchaseOrderTaxesConfiguration : BaseEntityConfiguration<PurchaseOrderTax>
    {
        public override void Configure(EntityTypeBuilder<PurchaseOrderTax> builder)
        {
            base.Configure(builder);
            builder
       .HasKey(st => new { st.PurchaseOrderId, st.TaxId });

            builder
                .HasOne(st => st.PurchaseOrder)
                .WithMany(so => so.PurchaseOrderTaxes)
                .HasForeignKey(st => st.PurchaseOrderId);

            builder
                .HasOne(st => st.Tax)
                .WithMany(t => t.PurchaseOrderTaxes)
                .HasForeignKey(st => st.TaxId);
        }
    }
}
