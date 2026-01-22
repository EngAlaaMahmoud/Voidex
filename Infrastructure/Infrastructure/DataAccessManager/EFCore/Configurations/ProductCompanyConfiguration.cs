using Domain.Entities;
using Infrastructure.DataAccessManager.EFCore.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Domain.Common.Constants;

namespace Infrastructure.DataAccessManager.EFCore.Configurations;

public class ProductCompanyConfiguration : BaseEntityConfiguration<ProductCompany>
{
    public override void Configure(EntityTypeBuilder<ProductCompany> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).HasMaxLength(NameConsts.MaxLength).IsRequired(true);
        builder.Property(x => x.Description).HasMaxLength(DescriptionConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.Street).HasMaxLength(NameConsts.MaxLength).IsRequired(false);
        builder.Property(x => x.City).HasMaxLength(NameConsts.MaxLength).IsRequired(false);

        builder.HasIndex(e => e.Name);
    }
}
