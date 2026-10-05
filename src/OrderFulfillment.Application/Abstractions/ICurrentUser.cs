namespace OrderFulfillment.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAdministrator { get; }
}