using Xunit;

namespace OrderFulfillment.IntegrationTests.Support;

[CollectionDefinition(Name)]
public sealed class PostgresCollectionDefinition : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}