namespace Boson;

public abstract record Result
{
    public abstract void Unwrap();
    public bool IsOk => this is Ok;

    public static Result Wrap(Action action)
    {
        try
        {
            action();
            return new Ok();
        }
        catch (Exception ex)
        {
            return new Error(ex);
        }
    }
}
public record Ok : Result
{
    public override void Unwrap() { }
}
public record Error(Exception Exception) : Result
{
    public override void Unwrap() => throw Exception;
}

public abstract record Result<T>
{
    public abstract T Unwrap();
    public bool IsOk => this is Ok<T>;

    public static Result<T> Wrap(Func<T> func)
    {
        try
        {
            return new Ok<T>(func());
        }
        catch (Exception ex)
        {
            return new Error<T>(ex);
        }
    }
}
public record Ok<T>(T Value) : Result<T>
{
    public override T Unwrap() => Value;
}
public record Error<T>(Exception Exception) : Result<T>
{
    public override T Unwrap() => throw Exception;
}