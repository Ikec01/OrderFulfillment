namespace OrderFulfillment.Application.Common;

public sealed class Result
{
    private Result(bool isSuccess, ApplicationError error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ApplicationError Error { get; }

    public static Result Success() => new(true, ApplicationError.None);

    public static Result Failure(ApplicationError error)
    {
        EnsureValidError(error);

        return new Result(false, error);
    }

    public static Result<T> Success<T>(T value) => new(value, true, ApplicationError.None);

    public static Result<T> Failure<T>(ApplicationError error)
    {
        EnsureValidError(error);

        return new Result<T>(default, false, error);
    }

    private static void EnsureValidError(ApplicationError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error == ApplicationError.None)
        {
            throw new ArgumentException("Neuspeh mora imati grešku.", nameof(error));
        }
    }
}