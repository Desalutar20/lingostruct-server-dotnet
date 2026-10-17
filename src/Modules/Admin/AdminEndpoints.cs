using src.Modules.Admin.Users.Endpoints;
using src.Modules.Admin.Workspaces.Endpoints;

namespace src.Modules.Admin;

internal static partial class AdminEndpoints
{
    public static void MapAdminV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/admin").WithTags("Admin");

        group.MapAdminUsersV1();
        group.MapAdminWorkspacesV1();
    }
}
