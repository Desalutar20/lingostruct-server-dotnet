namespace Integration.Auth;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Setup;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Auth.Endpoints;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;
using Xunit;

public sealed class SignUpTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await GetUserFromDbByEmail(Email.Create(request.Email).Value, ct);

        user.Should().NotBeNull();
        user.IsVerified.Should().BeFalse();
        user.HashedPassword!.Value.Should().NotBeEquivalentTo(request.Password);
    }

    [Fact]
    public async ValueTask Should_Store_Verification_Token()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var verificationToken = await GetRedisToken(TokenType.AccountVerification, ct);
        verificationToken.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        SignUpRequest invalidRequest,
        string because
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await SignUp(invalidRequest, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    public static TheoryData<string, SignUpRequest, string> InvalidRequests()
    {
        var request = TestData.SignUpRequest();

        return
        [
            ("email", request with { Email = "invalid email" }, "Invalid email"),
            ("email", request with { Email = "" }, "Empty email"),
            ("password", request with { Password = "" }, "Empty password"),
            (
                "password",
                request with
                {
                    Password = TestData.FakePassword(Password.MinLength - 1),
                },
                "Too short password"
            ),
            (
                "password",
                request with
                {
                    Password = TestData.FakePassword(Password.MaxLength + 1),
                },
                "Too long password"
            ),
            ("firstName", request with { FirstName = "" }, "Empty first name"),
            (
                "firstName",
                request with
                {
                    FirstName = new string('p', FirstName.MaxLength + 1),
                },
                "Too long first name"
            ),
            ("lastName", request with { LastName = "" }, "Empty last name"),
            (
                "lastName",
                request with
                {
                    LastName = new string('p', LastName.MaxLength + 1),
                },
                "Too long last name"
            ),
        ];
    }
}
