namespace Application.Common.Results;

public class AppResult<T> : AppResult
{
    private readonly T? _value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of failed result.");

    public bool HasValue => IsSuccess && _value is not null;

    public override string ToString() =>
        IsSuccess ? $"Success({typeof(T).Name})" : $"Failure({Error?.Code})";

    private AppResult(bool isSuccess, T? value, AppError? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static AppResult<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
        }
        return new(true, value, null);
    }

    public new static AppResult<T> Failure(AppError error) =>
        new(false, default, error ?? throw new ArgumentNullException(nameof(error)));

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<AppError, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(Value) : onFailure(Error!);
    }

    public void Match(Action<T> onSuccess, Action<AppError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess) onSuccess(Value);
        else onFailure(Error!);
    }
}
