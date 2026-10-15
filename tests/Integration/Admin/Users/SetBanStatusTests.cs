using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Setup;
using src.Modules.Admin.Users.Dto;
using src.Modules.Admin.Users.Endpoints;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Users.Domain;
using Xunit;

namespace Integration.Admin.Users;

public sealed class SetBanStatusTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, UserRole.Admin);

        var response = await GetAdminUsers(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiKeysetPaginatedResponse<AdminUserDto>
        >(ct);
        body.Should().NotBeNull();

        var setBanStatusResponse = await SetBanStatus(
            body.Data[0].Id,
            new SetBanStatusRequest(true),
            sessionCookie,
            ct
        );
        setBanStatusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbUser = await GetUserFromDbById(new UserId(body.Data[0].Id), ct);
        dbUser.Should().NotBeNull();
        dbUser.IsBanned.Should().BeTrue();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_UserNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, UserRole.Admin);

        var response = await SetBanStatus(
            Guid.NewGuid(),
            new SetBanStatusRequest(true),
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await SetBanStatus(Guid.NewGuid(), new SetBanStatusRequest(true), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await SetBanStatus(
            Guid.NewGuid(),
            new SetBanStatusRequest(true),
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
