namespace OrderFulfillment.Application.Common;

public sealed class Result<T>
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, ApplicationError error)
    {
        _value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ApplicationError Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Neuspešan rezultat nema vrednost.");
}