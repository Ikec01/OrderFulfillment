using OrderFulfillment.Application.Abstractions;

namespace OrderFulfillment.Application.UnitTests.Fakes;

internal sealed class FakeCurrentUser : ICurrentUser
{
    private FakeCurrentUser(Guid? userId, bool isAdministrator)
    {
        UserId = userId;
        IsAdministrator = isAdministrator;
    }

    public Guid? UserId { get; }

    public bool IsAdministrator { get; }

    public static FakeCurrentUser Customer() => new(Guid.NewGuid(), false);

    public static FakeCurrentUser Administrator() => new(Guid.NewGuid(), true);

    public static FakeCurrentUser Anonymous() => new(null, false);
}