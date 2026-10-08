using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Setup;
using src.Modules.Auth.Endpoints;
using src.Modules.Shared.Domain.ValueObjects;
using Xunit;

namespace Integration.Auth;

using Microsoft.AspNetCore.Mvc;

public sealed class VerifyAccountTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var signUpRequest = TestData.SignUpRequest();
        var signUpResponse = await SignUp(signUpRequest, ct);

        signUpResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        var request = new VerifyAccountRequest(token, signUpRequest.Email);
        var response = await VerifyAccount(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await GetUserFromDbByEmail(Email.Create(request.Email).Value, ct);

        user.Should().NotBeNull();
        user.IsVerified.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        VerifyAccountRequest invalidRequest,
        string because
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await VerifyAccount(invalidRequest, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    public static TheoryData<string, VerifyAccountRequest, string> InvalidRequests()
    {
        var request = new VerifyAccountRequest("token", TestData.FakeEmail());

        return
        [
            ("token", new VerifyAccountRequest("", TestData.FakeEmail()), "Empty token"),
            ("email", request with { Email = "invalid email" }, "Invalid email"),
            ("email", request with { Email = "" }, "Empty email"),
        ];
    }
}
