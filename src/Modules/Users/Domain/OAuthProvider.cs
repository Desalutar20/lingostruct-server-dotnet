using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record OAuthProvider : NonEmptyString
{
    public static OAuthProvider Google => new("google");
    public static OAuthProvider Github => new("github");

    private OAuthProvider(string value)
        : base(value) { }

    public static Result<OAuthProvider> Create(string value) =>
        value?.ToLowerInvariant() switch
        {
            "google" => Google,
            "github" => Github,
            _ => Error.Validation("provider", [$"Invalid oauth provider: {value}"]),
        };

    public override string ToString() => base.ToString();
};
