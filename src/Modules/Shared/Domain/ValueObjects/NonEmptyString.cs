namespace src.Modules.Shared.Domain.ValueObjects;

public record NonEmptyString
{
    public string Value { get; }

    protected NonEmptyString(string value)
    {
        Value = value;
    }

    public static Result<NonEmptyString> Create(
        string value,
        string field = "value",
        string label = "Value",
        int? maxLength = null,
        int? minLength = 1,
        Func<string, Result>? additionalCheck = null
    )
    {
        var result = BaseCheck(value, field, label, maxLength, minLength, additionalCheck);

        return result.IsFailure ? result.Error : new NonEmptyString(result.Value);
    }

    public static Result<T> Create<T>(
        string value,
        string field,
        string label,
        int maxLength,
        Func<string, T> factory,
        int? minLength = 1,
        Func<string, Result>? additionalCheck = null
    )
        where T : notnull
    {
        var result = BaseCheck(value, field, label, maxLength, minLength, additionalCheck);

        return result.IsFailure ? result.Error : factory(result.Value);
    }

    private static Result<string> BaseCheck(
        string value,
        string field,
        string label,
        int? maxLength,
        int? minLength,
        Func<string, Result>? additionalCheck
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation(field, [$"{label} can not be empty"]);
        }

        value = value.Trim();

        if (value.Length == 0)
        {
            return Error.Validation(field, [$"{label} can not be empty"]);
        }

        if (maxLength is not null && value.Length > maxLength.Value)
        {
            return Error.Validation(
                field,
                [$"{label} can not be longer than {maxLength.Value} characters"]
            );
        }

        if (minLength is not null && value.Length < minLength.Value)
        {
            return Error.Validation(
                field,
                [$"{label} can not be shorter than {minLength.Value} characters"]
            );
        }

        if (additionalCheck is not null)
        {
            var result = additionalCheck(value);
            if (result.IsFailure)
            {
                return result.Error;
            }
        }

        return value;
    }

    public override string ToString() => Value;
};
