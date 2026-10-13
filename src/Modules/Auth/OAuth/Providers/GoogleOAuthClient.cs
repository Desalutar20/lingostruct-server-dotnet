namespace src.Modules.Auth.OAuth.Providers;

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using src.Config;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

internal sealed record GoogleAccessTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken
);

internal sealed record GoogleUserResponse(
    [property: JsonPropertyName("sub")] string Sub,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("email_verified")] bool EmailVerified
);

internal sealed class GoogleOAuthClient(HttpClient httpClient, Config config)
    : OAuthClient(config.OAuth.Google),
        IOAuthClient
{
    public override Url GenerateRedirectUrl(OAuthState state)
    {
        var uri = new Uri("https://accounts.google.com/o/oauth2/v2/auth")
            .AddParameter("client_id", ClientId)
            .AddParameter("redirect_uri", RedirectUrl)
            .AddParameter("response_type", "code")
            .AddParameter("scope", "openid email profile")
            .AddParameter("access_type", "offline")
            .AddParameter("state", state.ToString());

        return Url.Create(uri.ToString()).Value;
    }

    public override async Task<Result<OAuthUser>> GetUserAsync(
        NonEmptyString code,
        CancellationToken ct = default
    )
    {
        try
        {
            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code.Value,
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = RedirectUrl,
                ["grant_type"] = "authorization_code",
            };

            var tokenResponse = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest),
                ct
            );
            tokenResponse.EnsureSuccessStatusCode();

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);

            var token =
                JsonSerializer.Deserialize<GoogleAccessTokenResponse>(tokenJson)
                ?? throw new Exception("Google token response is null");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openidconnect.googleapis.com/v1/userinfo"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.AccessToken
            );

            var userResponse = await httpClient.SendAsync(request, ct);
            userResponse.EnsureSuccessStatusCode();

            var userJson = await userResponse.Content.ReadAsStringAsync(ct);

            var user =
                JsonSerializer.Deserialize<GoogleUserResponse>(userJson)
                ?? throw new Exception("Google user response is null");

            if (!user.EmailVerified)
            {
                return Error.Conflict("Google user email is not verified");
            }

            var providerIdResult = ProviderId.Create(user.Sub);
            if (providerIdResult.IsFailure)
            {
                return providerIdResult.Error;
            }

            var emailResult = Email.Create(user.Email);
            if (emailResult.IsFailure)
            {
                return emailResult.Error;
            }

            return new OAuthUser(providerIdResult.Value, emailResult.Value);
        }
        catch (Exception ex)
        {
            throw new Exception($"Google OAuth failed: {ex.Message}", ex);
        }
    }
}
