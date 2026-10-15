using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using src.Modules.Auth.Endpoints;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;
using Xunit;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> SignUp(
        SignUpRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/sign-up", null, ct);

    protected async Task<HttpResponseMessage> SignIn(
        SignInRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/sign-in", null, ct);

    protected async Task<HttpResponseMessage> VerifyAccount(
        VerifyAccountRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/verify-account", null, ct);

    protected async Task<HttpResponseMessage> ForgotPassword(
        ForgotPasswordRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/forgot-password", null, ct);

    protected async Task<HttpResponseMessage> ResetPassword(
        ResetPasswordRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/reset-password", null, ct);

    protected async Task CreateAndVerify(SignUpRequest data, CancellationToken ct = default)
    {
        var signUpResponse = await SignUp(data, ct);
        signUpResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        var accountVerificationResponse = await VerifyAccount(
            new VerifyAccountRequest(token!, data.Email),
            ct
        );
        accountVerificationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    protected async Task<string> CreateAndSignIn(
        SignUpRequest data,
        CancellationToken ct = default,
        UserRole? role = null
    )
    {
        await CreateAndVerify(data, ct);

        if (role is not null)
        {
            await _dbContext
                .Users.Where(u => u.Email == data.Email)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.Role, user => role), ct);
        }

        var signInResponse = await SignIn(new SignInRequest(data.Email, data.Password), ct);
        signInResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var ok = signInResponse.Headers.TryGetValues("Set-Cookie", out var values);
        ok.Should().BeTrue();

        var sessionCookie = values!.FirstOrDefault(c =>
            c.StartsWith(Config.Application.SessionCookieName)
        );
        sessionCookie.Should().NotBeNull();

        return sessionCookie;
    }
}
