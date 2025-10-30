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
    public class SalesOrderTaxesConfiguration : BaseEntityConfiguration<SalesOrderTax>
    {
        public override void Configure(EntityTypeBuilder<SalesOrderTax> builder)
        {
            base.Configure(builder);
            builder
       .HasKey(st => new { st.SalesOrderId, st.TaxId });

            //builder
            //    .HasOne(st => st.SalesOrder)
            //    .WithMany(so => so.SalesOrderTaxes)
            //    .HasForeignKey(st => st.SalesOrderId);

            builder
                .HasOne(st => st.Tax)
                .WithMany(t => t.SalesOrderTaxes)
                .HasForeignKey(st => st.TaxId);
        }
    }
}
