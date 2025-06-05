# ADR 0014: Use of Result Pattern for Error Handling

## Status
Accepted

## Date
2025-06-03

## Context

During development of KedrStore, we needed a consistent strategy for error handling. The traditional approach in .NET uses exceptions, but this has several downsides:

1. Performance degradation due to exception handling  
2. Difficulty tracking all possible exceptions  
3. Using exceptions for control flow is an anti-pattern  
4. Exceptions may obscure business error semantics  
5. Overuse of try-catch leads to tangled, error-prone code  

We evaluated several alternatives:

1. Classic exceptions  
2. Error codes (legacy style)  
3. Tuples or out-parameters  
4. **Result Pattern**  
5. Monadic Option/Maybe style  

## Decision

We chose to adopt the **Result Pattern** in the Application layer.

It encapsulates success or failure within a return type rather than relying on exceptions.

Core structure:

```csharp
public class Result<TValue>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TValue Value { get; }
    public Error Error { get; }

    private Result(TValue value, bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TValue> Success(TValue value) =>
        new Result<TValue>(value, true, Error.None);

    public static Result<TValue> Failure(Error error) =>
        new Result<TValue>(default, false, error);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, Error.None);
    public static Result Failure(Error error) => new Result(false, error);

    public static Result<TValue> From<TValue>(Result result, TValue value) =>
        result.IsSuccess ? Result<TValue>.Success(value) : Result<TValue>.Failure(result.Error);
}

public record Error(string Code, string Message)
{
    public static readonly Error None = new Error(string.Empty, string.Empty);
}
```

## Usage Example

In a command handler:

```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.Items.Count == 0)
        {
            return Result<OrderDto>.Failure(new Error(
                "Order.EmptyItems",
                "Order cannot be empty"
            ));
        }

        try
        {
            var order = new Order(command.CustomerId, command.Items);
            await _orderRepository.AddAsync(order, cancellationToken);

            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating order");
            return Result<OrderDto>.Failure(new Error(
                "Order.Creation.Failed",
                "Internal error while creating order"
            ));
        }
    }
}
```

In a controller or Blazor page:

```csharp
var result = await _mediator.Send(command);
if (result.IsFailure)
{
    return BadRequest(new { error = result.Error });
}
return Ok(result.Value);
```

## Consequences

### Positive

- Explicit error contracts in method signatures  
- Unified approach to error handling without exceptions  
- Predictable execution flow  
- Avoids forgotten null-checks  
- Stronger type safety  

### Negative

- More boilerplate required to propagate results  
- Need to check `IsSuccess` manually  
- Developers less familiar with Result semantics  

## Extensions and Helpers

We added some utility methods:

```csharp
public static class ResultExtensions
{
    public static TResult Match<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    public static Result<TResult> Map<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> mapper) =>
        result.IsSuccess
            ? Result<TResult>.Success(mapper(result.Value))
            : Result<TResult>.Failure(result.Error);

    public static Result<TResult> Bind<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Result<TResult>> binder) =>
        result.IsSuccess ? binder(result.Value) : Result<TResult>.Failure(result.Error);

    public static Result<TValue> OnFailure<TValue>(
        this Result<TValue> result,
        Action<Error> action)
    {
        if (result.IsFailure)
        {
            action(result.Error);
        }

        return result;
    }
}
```

## Layer Usage

- **Domain Layer**: throws exceptions for invariants and rules  
- **Application Layer**: uses `Result<T>` for business logic  
- **Infrastructure Layer**: translates infrastructure errors into `Result`  
- **Presentation Layer**: converts `Result` into HTTP/UI responses  
