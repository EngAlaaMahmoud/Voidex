using Domain.Entities;
using Infrastructure.DataAccessManager.EFCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Domain.Common.Constants;

namespace Infrastructure.DataAccessManager.EFCore.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).HasMaxLength(NameConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.Number).HasMaxLength(CodeConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.Description).HasMaxLength(DescriptionConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.UnitPrice).IsRequired(false);
        builder.Property(x => x.Physical).IsRequired(false);
        builder.Property(x => x.UnitMeasureId).HasMaxLength(IdConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.ProductGroupId).HasMaxLength(IdConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.InternalCode).HasMaxLength(CodeConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.GisEgsCode).HasMaxLength(CodeConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.CompanyName).HasMaxLength(NameConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.Model).HasMaxLength(NameConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.Discount).IsRequired(false);
        builder.Property(x => x.PriceAfterDiscount).IsRequired(false);
        builder.Property(x => x.ServiceFee).IsRequired(false);
        builder.Property(x => x.AdditionalTax).IsRequired(false);
        builder.Property(x => x.AdditionalFee).IsRequired(false);

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Number);
        builder.HasIndex(e => e.InternalCode);

        builder.HasOne(p => p.Vat)
          .WithMany(v => v.Products)
          .HasForeignKey(p => p.VatId)
          .OnDelete(DeleteBehavior.Restrict); // or your preferred delete behavior

        //builder.HasOne(p => p.Tax)
        //    .WithMany(t => t.Products)
        //    .HasForeignKey(p => p.TaxId)
        //    .OnDelete(DeleteBehavior.Restrict);
    }
}

