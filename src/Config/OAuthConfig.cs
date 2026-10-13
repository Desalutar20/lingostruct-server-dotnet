namespace src.Config;

public sealed record OAuthConfig
{
    public required OAuthProviderConfig Google { get; init; }
    public required OAuthProviderConfig Github { get; init; }
}

public sealed record OAuthProviderConfig
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUrl { get; init; }
}
