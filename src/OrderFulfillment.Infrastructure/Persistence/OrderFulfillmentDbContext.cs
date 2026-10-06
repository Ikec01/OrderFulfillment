using Microsoft.EntityFrameworkCore;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Infrastructure.Persistence.Outbox;

namespace OrderFulfillment.Infrastructure.Persistence;

public sealed class OrderFulfillmentDbContext(DbContextOptions<OrderFulfillmentDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var aggregates = AddOutboxMessagesForDomainEvents();

        var result = base.SaveChanges(acceptAllChangesOnSuccess);

        ClearDomainEvents(aggregates);

        return result;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var aggregates = AddOutboxMessagesForDomainEvents();

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        ClearDomainEvents(aggregates);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFulfillmentDbContext).Assembly);
    }

    private List<IAggregateRoot> AddOutboxMessagesForDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        foreach (var domainEvent in aggregates.SelectMany(aggregate => aggregate.DomainEvents))
        {
            OutboxMessages.Add(OutboxMessage.FromDomainEvent(domainEvent));
        }

        return aggregates;
    }

    private static void ClearDomainEvents(List<IAggregateRoot> aggregates)
    {
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
    }
}