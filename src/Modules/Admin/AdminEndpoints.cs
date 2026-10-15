using src.Modules.Admin.Users.Endpoints;

namespace src.Modules.Admin;

internal static partial class AdminEndpoints
{
    public static void MapAdminV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/admin");

        group.MapAdminUsersV1();
    }
}
