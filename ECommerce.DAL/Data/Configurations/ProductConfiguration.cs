using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DAL.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(product => product.Description)
            .HasMaxLength(1000);

        builder.Property(product => product.ImageUrl)
            .HasMaxLength(500);

        builder.Property(product => product.Price)
            .HasPrecision(18, 2);

        builder.Property(product => product.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(product => product.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(product => product.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasQueryFilter(product => !product.IsDeleted);

        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
