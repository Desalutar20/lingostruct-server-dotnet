using Microsoft.VisualBasic;

namespace src.Modules.Shared.Domain.Pagination;

internal sealed record KeysetCursor<TId>(TId Id, DateTimeOffset CreatedAt)
    where TId : notnull
{
    private const char CursorSeparator = '|';

    public static Result<KeysetCursor<TId>> Create(string value, Func<string, Result<TId>> parse)
    {
        var errors = new List<string>();

        string decoded;

        try
        {
            decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch (Exception)
        {
            return Error.Validation("cursor", ["Invalid cursor"]);
        }

        var parts = decoded.Split(CursorSeparator);
        if (parts.Length != 2)
        {
            return Error.Validation("cursor", ["Invalid cursor"]);
        }

        var parsedResult = parse(parts[0]);
        if (parsedResult.IsFailure)
        {
            errors.Add("Invalid cursor id format");
        }

        if (
            !DateTimeOffset.TryParseExact(
                parts[1],
                "O",
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var date
            )
        )
        {
            errors.Add("Invalid cursor date format");
        }

        return errors.Count == 0
            ? new KeysetCursor<TId>(parsedResult.Value, date)
            : Error.Validation("cursor", errors);
    }

    public override string ToString()
    {
        var fullValue = $"{Id}{CursorSeparator}{CreatedAt:O}";
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fullValue);

        return Convert.ToBase64String(plainTextBytes);
    }
};
