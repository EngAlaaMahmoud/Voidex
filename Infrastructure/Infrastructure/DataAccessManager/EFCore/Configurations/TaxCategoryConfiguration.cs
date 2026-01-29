using Domain.Entities;
using Infrastructure.DataAccessManager.EFCore.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Domain.Common.Constants;

namespace Infrastructure.DataAccessManager.EFCore.Configurations
{
    public class TaxCategoryConfiguration : BaseEntityConfiguration<TaxCategory>
    {
        public override void Configure(EntityTypeBuilder<TaxCategory> builder)
        {
            base.Configure(builder);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Code).HasMaxLength(10).IsRequired();
            builder.Property(e => e.NameAr).HasMaxLength(150).IsRequired();
            builder.HasIndex(e => e.Code).IsUnique();
         
        }
    }
}
