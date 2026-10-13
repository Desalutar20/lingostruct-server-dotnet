namespace src.Modules.Shared.Domain.ValueObjects;

public sealed record Url : NonEmptyString
{
    private Url(string value)
        : base(value) { }

    public static Result<Url> Create(string value, string field = "url") =>
        NonEmptyString.Create<Url>(
            value,
            field,
            label: "URL",
            maxLength: 2048,
            (val) => new Url(val),
            additionalCheck: IsValidUrl
        );

    private static Result IsValidUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return Error.Validation("url", ["Invalid URL"]);
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            return Error.Validation("url", ["Invalid URL"]);
        }

        return Result.Success();
    }

    public override string ToString() => base.ToString();
}
