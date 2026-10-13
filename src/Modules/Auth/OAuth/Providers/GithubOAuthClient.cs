using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.OAuth.Providers;

internal sealed record GithubAccessTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken
);

internal sealed record GithubUserResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("email")] string? Email
);

internal sealed record GithubUserEmailResponse(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("primary")] bool Primary,
    [property: JsonPropertyName("verified")] bool Verified
);

internal sealed class GithubOAuthClient(HttpClient httpClient, Config.Config config)
    : OAuthClient(config.OAuth.Github),
        IOAuthClient
{
    public override Url GenerateRedirectUrl(OAuthState state)
    {
        var uri = new Uri("https://github.com/login/oauth/authorize")
            .AddParameter("client_id", ClientId)
            .AddParameter("redirect_uri", RedirectUrl)
            .AddParameter("scope", "read:user user:email")
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
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://github.com/login/oauth/access_token"
            );
            request.Content = JsonContent.Create(tokenRequest);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var tokenResponse = await httpClient.SendAsync(request, ct);
            tokenResponse.EnsureSuccessStatusCode();

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);

            var token =
                JsonSerializer.Deserialize<GithubAccessTokenResponse>(tokenJson)
                ?? throw new Exception("Github token response is null");

            using var userInfoRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.github.com/user"
            );

            userInfoRequest.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json")
            );
            userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.AccessToken
            );
            userInfoRequest.Headers.Add("X-Github-Api-Version", ["2026-03-10"]);

            var userResponse = await httpClient.SendAsync(userInfoRequest, ct);
            userResponse.EnsureSuccessStatusCode();

            var userJson = await userResponse.Content.ReadAsStringAsync(ct);

            var user =
                JsonSerializer.Deserialize<GithubUserResponse>(userJson)
                ?? throw new Exception("Github user response is null");

            var providerIdResult = ProviderId.Create(user.Id.ToString());
            if (providerIdResult.IsFailure)
            {
                return providerIdResult.Error;
            }

            if (user.Email is not null)
            {
                var emailResult = Email.Create(user.Email);
                if (emailResult.IsFailure)
                {
                    return emailResult.Error;
                }

                return new OAuthUser(providerIdResult.Value, emailResult.Value);
            }

            using var emailsRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.github.com/user/emails"
            );

            emailsRequest.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json")
            );
            emailsRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.AccessToken
            );

            var emailsResponse = await httpClient.SendAsync(emailsRequest, ct);
            emailsResponse.EnsureSuccessStatusCode();

            var emailsJson = await emailsResponse.Content.ReadAsStringAsync(ct);

            var emails =
                JsonSerializer.Deserialize<GithubUserEmailResponse[]>(emailsJson)
                ?? throw new Exception("Failed to validate Github OAuth user emails response");

            var email = emails.FirstOrDefault(email =>
                (email.Primary && email.Verified)
                || (email.Verified && !email.Email.EndsWith("github.com"))
            );

            if (email is null)
            {
                return Error.Failure("GitHub OAuth account has no verified primary email");
            }

            var result = Email.Create(email.Email);
            if (result.IsFailure)
            {
                return result.Error;
            }

            return new OAuthUser(providerIdResult.Value, result.Value);
        }
        catch (Exception ex)
        {
            throw new Exception($"Github OAuth failed: {ex.Message}", ex);
        }
    }
}
