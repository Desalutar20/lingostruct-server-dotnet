namespace src.Modules.Shared.Domain;

public readonly record struct Optional<T>
{
    public bool HasValue { get; }

    private Optional(T value)
    {
        Value = value;
        HasValue = true;
    }

    public T Value =>
        HasValue
            ? field
            : throw new InvalidOperationException("Cannot access value when Optional is empty.");

    public static Optional<T> None => default;

    public static Optional<T> Some(T value) => new(value);
}
