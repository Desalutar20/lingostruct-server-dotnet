using src.Modules.Admin.Users.Endpoints;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> GetAdminUsers(
        GetAdminUsersRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = data?.Search,
            ["isBanned"] = data?.IsBanned?.ToString(),
            ["isVerified"] = data?.IsVerified?.ToString(),
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/admin/users", cookie, query, ct);
    }
}
