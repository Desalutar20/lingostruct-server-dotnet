namespace src.Modules.Shared.Domain.ValueObjects;

internal sealed record PositiveInt
{
    private PositiveInt(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<PositiveInt> Create(
        int value,
        string field = "value",
        string label = "Value"
    )
    {
        if (value < 1 || value > int.MaxValue)
        {
            return Error.Validation(field, [$"{label} must be a positive integer."]);
        }

        return new PositiveInt(value);
    }

    public static implicit operator int(PositiveInt positiveInt) => positiveInt.Value;

    public static PositiveInt operator +(PositiveInt left, PositiveInt right) =>
        new(left.Value + right.Value);

    public static PositiveInt operator -(PositiveInt left, PositiveInt right) =>
        new(left.Value - right.Value);

    public static PositiveInt operator *(PositiveInt left, PositiveInt right) =>
        new(left.Value * right.Value);

    public static PositiveInt operator /(PositiveInt left, PositiveInt right) =>
        new(left.Value / right.Value);

    public static bool operator >(PositiveInt left, PositiveInt right) => left.Value > right.Value;

    public static bool operator <(PositiveInt left, PositiveInt right) => left.Value < right.Value;

    public static bool operator >=(PositiveInt left, PositiveInt right) =>
        left.Value >= right.Value;

    public static bool operator <=(PositiveInt left, PositiveInt right) =>
        left.Value <= right.Value;

    public override string ToString() => Value.ToString();
}
