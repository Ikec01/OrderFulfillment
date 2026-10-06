using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Infrastructure.Persistence.Configurations
{
    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("order_items");

            builder.HasKey(item => item.Id);

            builder.Property(item => item.Id)
                .HasConversion(id => id.Value, value => new OrderItemId(value))
                .ValueGeneratedNever();

            builder.Property(item => item.ProductId)
                .HasConversion(id => id.Value, value => new ProductId(value))
                .IsRequired();

            builder.Property(item => item.ProductName)
                .HasMaxLength(OrderItem.MaxProductNameLength)
                .IsRequired();

            builder.Property(item => item.Quantity).IsRequired();

            builder.OwnsOne(item => item.UnitPrice, money => 
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
            });

            builder.Navigation(item => item.UnitPrice).IsRequired();

            builder.Ignore(item => item.ToralPrice);
        }
    }
}
