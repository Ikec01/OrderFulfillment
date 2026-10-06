using System.Reflection;
using System.Text.Json;
using OrderFulfillment.Contracts.Orders;

namespace OrderFulfillment.Infrastructure.Persistence.Outbox;

public static class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static readonly Assembly ContractsAssembly = typeof(OrderPlacedIntegrationEvent).Assembly;

    public static string Serialize(object integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), Options);
    }

    public static object Deserialize(string typeName, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        // Dozvoljeni su samo tipovi iz Contracts projekta, nikad proizvoljan tip upisan u bazu.
        var type = ContractsAssembly.GetType(typeName, throwOnError: false)
            ?? throw new InvalidOperationException($"Nepoznat tip integracionog događaja '{typeName}'.");

        return JsonSerializer.Deserialize(content, type, Options)
            ?? throw new InvalidOperationException($"Sadržaj poruke tipa '{typeName}' je prazan.");
    }
}