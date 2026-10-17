namespace src.Modules.Admin.Workspaces.Endpoints;

internal static partial class AdminWorkspacesEndpoints
{
    public static void MapAdminWorkspacesV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/workspaces").WithTags("Admin workspaces");

        group.GetAdminWorkspacesV1().CreateWorkspaceV1();
    }
}
