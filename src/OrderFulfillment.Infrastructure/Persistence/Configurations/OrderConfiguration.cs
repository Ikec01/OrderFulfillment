using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;


namespace OrderFulfillment.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("orders");

            builder.HasKey(order => order.Id);

            builder.Property(order => order.Id)
                .HasConversion(id => id.Value, value => new OrderId(value))
                .ValueGeneratedNever();

            builder.Property(order => order.CustomerId)
                .HasConversion(id => id.Value, value => new CustomerId(value))
                .IsRequired();

            builder.HasIndex(order => order.CustomerId);

            builder.Property(order => order.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(order => order.Status).IsRequired();

            builder.Property(order => order.CreatedAtUtc).IsRequired();

            builder.OwnsOne(order => order.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasMaxLength(Address.MaxStreetLenght).IsRequired();
                address.Property(a => a.City).HasMaxLength(Address.MaxCityLength).IsRequired();
                address.Property(a => a.PostalCode).HasMaxLength(Address.MaxPostalCodeLength).IsRequired();
                address.Property(a => a.Country).HasMaxLength(Address.MaxCountryLength).IsRequired();
            });

            builder.Navigation(order => order.ShippingAddress).IsRequired();

            builder.HasMany(order => order.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(order => order.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property<uint>("xmin").IsRowVersion();

            builder.Ignore(order => order.DomainEvents);
            builder.Ignore(order => order.TotalAmount);
        }
    }
}
