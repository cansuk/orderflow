using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        builder.Property(o => o.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(500);
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsMany(o => o.Items, itemBuilder =>
        {
            itemBuilder.ToTable("order_items");
            itemBuilder.WithOwner().HasForeignKey("order_id");
            itemBuilder.Property<int>("id").ValueGeneratedOnAdd();
            itemBuilder.HasKey("id");

            itemBuilder.Property(i => i.ProductId).HasColumnName("product_id");
            itemBuilder.Property(i => i.ProductName).HasColumnName("product_name").HasMaxLength(200);
            itemBuilder.Property(i => i.Quantity).HasColumnName("quantity");
            itemBuilder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
        });

        builder.Ignore(o => o.DomainEvents);
        builder.HasIndex(o => o.CustomerId).HasDatabaseName("ix_orders_customer_id");
        builder.HasIndex(o => o.Status).HasDatabaseName("ix_orders_status");
    }
}
