using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFulfillment.Infrastructure.Persistence.Outbox;

namespace OrderFulfillment.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(OutboxMessage.MaxTypeLength)
            .IsRequired();

        builder.Property(message => message.Content)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.OccurredOnUtc).IsRequired();

        builder.Property(message => message.Error).HasMaxLength(OutboxMessage.MaxErrorLength);

        builder.HasIndex(message => message.OccurredOnUtc)
            .HasFilter("\"ProcessedOnUtc\" IS NULL")
            .HasDatabaseName("ix_outbox_messages_unprocessed");
    }
}