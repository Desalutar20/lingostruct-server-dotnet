namespace Integration.Admin.Workspaces;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Setup;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Admin.Workspaces.Constants;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Admin.Workspaces.Endpoints;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Users.Domain;
using Xunit;

public sealed class GetAdminWorkspacesTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, UserRole.Admin);

        var response = await GetAdminWorkspaces(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiKeysetPaginatedResponse<AdminWorkspaceDto>
        >(ct);

        body.Should().NotBeNull();
        body.Data.Should().HaveCount(AdminWorkspacesConstants.GetAdminWorkspacesDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetAdminWorkspacesRequest invalidRequest,
        string because
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, UserRole.Admin);

        var response = await GetAdminWorkspaces(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new GetAdminWorkspacesRequest(null, null, null, null);

        var response = await GetAdminWorkspaces(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var getAdminWorkspacesRequest = new GetAdminWorkspacesRequest(null, null, null, null);

        var response = await GetAdminWorkspaces(getAdminWorkspacesRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, GetAdminWorkspacesRequest, string> InvalidRequests()
    {
        var request = new GetAdminWorkspacesRequest(null, null, null, null);

        return
        [
            (
                "search",
                request with
                {
                    Search = TestData.String(
                        AdminWorkspacesConstants.GetAdminWorkspacesSearchMaxLength + 1
                    ),
                },
                "Too long search"
            ),
            ("limit", request with { Limit = 0 }, "Limit is 0"),
            (
                "limit",
                request with
                {
                    Limit = AdminWorkspacesConstants.GetAdminWorkspacesMaxLimit + 1,
                },
                "Limit it too high"
            ),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(
                        AdminWorkspacesConstants.GetAdminWorkspacesCursorMaxLength + 1
                    ),
                },
                "Too long prev cursor"
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(
                        AdminWorkspacesConstants.GetAdminWorkspacesCursorMaxLength + 1
                    ),
                },
                "Too long next cursor"
            ),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = "prev",
                    NextCursor = "next",
                },
                "Both prev and next cursor are provided"
            ),
        ];
    }
}
