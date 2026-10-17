namespace Integration.Admin.Workspaces;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Setup;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Admin.Users.Endpoints;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Admin.Workspaces.Endpoints;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Users.Domain;
using src.Modules.Workspaces.Domain;
using Xunit;

public sealed class CreateWorkspaceTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, UserRole.Admin);

        var response = await CreateWorkspace(TestData.CreateWorkspaceRequest(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AdminWorkspaceDto>>(ct);
        body.Should().NotBeNull();

        var dbWorkspace = await GetWorkspaceFromDbById(new WorkspaceId(body.Data.Id), ct);
        dbWorkspace.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateWorkspaceRequest invalidRequest,
        string because
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, UserRole.Admin);

        var response = await CreateWorkspace(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
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

    public static TheoryData<string, CreateWorkspaceRequest, string> InvalidRequests()
    {
        var request = TestData.CreateWorkspaceRequest();

        return
        [
            ("name", request with { Name = "" }, "Empty name"),
            (
                "name",
                request with
                {
                    Name = TestData.String(WorkspaceName.MaxLength + 1),
                },
                "Too long name"
            ),
            ("country", request with { Country = "" }, "Empty country"),
            (
                "country",
                request with
                {
                    Country = TestData.String(WorkspaceAddress.CountryMaxLength + 1),
                },
                "Too long country"
            ),
            ("city", request with { City = "" }, "Empty city"),
            (
                "city",
                request with
                {
                    City = TestData.String(WorkspaceAddress.CityMaxLength + 1),
                },
                "Too long city"
            ),
            ("street", request with { Street = "" }, "Empty street"),
            (
                "street",
                request with
                {
                    Street = TestData.String(WorkspaceAddress.StreetMaxLength + 1),
                },
                "Too long street"
            ),
            ("streetNumber", request with { StreetNumber = "" }, "Empty street number"),
            (
                "streetNumber",
                request with
                {
                    StreetNumber = TestData.String(WorkspaceAddress.StreetNumberMaxLength + 1),
                },
                "Too long street number"
            ),
            ("postalCode", request with { PostalCode = "" }, "Empty postal code"),
            (
                "postalCode",
                request with
                {
                    PostalCode = TestData.String(WorkspaceAddress.PostalCodeMaxLength + 1),
                },
                "Too long postal code"
            ),
        ];
    }
}
