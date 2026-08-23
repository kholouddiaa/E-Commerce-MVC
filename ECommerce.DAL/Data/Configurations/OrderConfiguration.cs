using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DAL.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(order => order.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(order => order.OrderDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(order => order.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(order => order.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(order => order.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasOne(order => order.User)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
