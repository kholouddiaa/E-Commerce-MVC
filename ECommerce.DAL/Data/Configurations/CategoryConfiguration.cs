using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DAL.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(category => category.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(category => category.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasQueryFilter(category => !category.IsDeleted);
    }
}
