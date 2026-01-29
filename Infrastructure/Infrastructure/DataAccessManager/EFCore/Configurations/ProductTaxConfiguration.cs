using Domain.Entities;
using Infrastructure.DataAccessManager.EFCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccessManager.EFCore.Configurations
{
    public class ProductTaxConfiguration : BaseEntityConfiguration<ProductTax>
    {
        public override void Configure(EntityTypeBuilder<ProductTax> builder)
        {
            builder.HasKey(pt => new { pt.ProductId, pt.TaxId });

            builder.HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTaxes)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pt => pt.Tax)
                .WithMany(t => t.ProductTaxes)
                .HasForeignKey(pt => pt.TaxId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}


