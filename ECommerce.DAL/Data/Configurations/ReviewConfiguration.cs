using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DAL.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(review => review.ProductId)
            .IsRequired();

        builder.Property(review => review.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(review => review.Rating)
            .IsRequired();

        builder.Property(review => review.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(review => review.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(review => review.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(review => new { review.UserId, review.ProductId })
            .IsUnique();

        builder.HasOne(review => review.Product)
            .WithMany(product => product.Reviews)
            .HasForeignKey(review => review.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(review => review.User)
            .WithMany(user => user.Reviews)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
