using System.Diagnostics.CodeAnalysis;
using Dapper;
using Npgsql;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Orders.Queries;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Infrastructure.Queries;

public sealed class OrderReadService(NpgsqlDataSource dataSource) : IOrderReadService
{
    private const string OrderDetailsSql = """
        SELECT o."Id",
               o."CustomerId",
               o."Status",
               o."Currency",
               o."CreatedAtUtc",
               o."ShippingAddress_Street" AS "Street",
               o."ShippingAddress_City" AS "City",
               o."ShippingAddress_PostalCode" AS "PostalCode",
               o."ShippingAddress_Country" AS "Country"
        FROM orders o
        WHERE o."Id" = @OrderId;

        SELECT i."ProductId",
               i."ProductName",
               i."UnitPrice_Amount" AS "UnitPrice",
               i."Quantity"
        FROM order_items i
        WHERE i."OrderId" = @OrderId
        ORDER BY i."ProductName", i."Id";
        """;

    private const string ListByCustomerSql = """
        SELECT o."Id",
               o."Status",
               o."Currency",
               COALESCE(SUM(i."UnitPrice_Amount" * i."Quantity"), 0) AS "TotalAmount",
               COUNT(i."Id")::int AS "ItemCount",
               o."CreatedAtUtc"
        FROM orders o
        LEFT JOIN order_items i ON i."OrderId" = o."Id"
        WHERE o."CustomerId" = @CustomerId
        GROUP BY o."Id"
        ORDER BY o."CreatedAtUtc" DESC, o."Id"
        LIMIT @PageSize OFFSET @Offset;
        """;

    public async Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            OrderDetailsSql,
            new { OrderId = orderId },
            cancellationToken: cancellationToken);

        using var grid = await connection.QueryMultipleAsync(command);

        var order = await grid.ReadSingleOrDefaultAsync<OrderRow>();

        if (order is null)
        {
            return null;
        }

        var items = (await grid.ReadAsync<OrderItemRow>())
            .Select(item => new OrderItemDto(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity,
                item.UnitPrice * item.Quantity))
            .ToList();

        return new OrderDetailsDto(
            order.Id,
            order.CustomerId,
            order.Status,
            order.Currency,
            items.Sum(item => item.TotalPrice),
            order.CreatedAtUtc,
            new OrderAddressDto(order.Street, order.City, order.PostalCode, order.Country),
            items);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> ListByCustomerAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var offset = (long)(page - 1) * pageSize;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            ListByCustomerSql,
            new { CustomerId = customerId, PageSize = pageSize, Offset = offset },
            cancellationToken: cancellationToken);

        var summaries = await connection.QueryAsync<OrderSummaryDto>(command);

        return summaries.ToList();
    }

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instancira ga Dapper.")]
    private sealed record OrderRow(
        Guid Id,
        Guid CustomerId,
        OrderStatus Status,
        string Currency,
        DateTime CreatedAtUtc,
        string Street,
        string City,
        string PostalCode,
        string Country);

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instancira ga Dapper.")]
    private sealed record OrderItemRow(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity);
}