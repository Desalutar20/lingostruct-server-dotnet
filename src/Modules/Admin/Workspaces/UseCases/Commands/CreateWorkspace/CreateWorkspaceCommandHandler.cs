using src.Data;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Workspaces.Domain;

namespace src.Modules.Admin.Workspaces.UseCases.Commands.CreateWorkspace;

internal sealed record CreateWorkspaceCommand(WorkspaceName Name, WorkspaceAddress Address)
    : ICommand<AdminWorkspaceDto>;

internal sealed class CreateWorkspaceCommandHandler(AppDbContext dbContext)
    : ICommandHandler<CreateWorkspaceCommand, AdminWorkspaceDto>
{
    public async Task<Result<AdminWorkspaceDto>> Handle(
        CreateWorkspaceCommand command,
        CancellationToken ct = default
    )
    {
        var workspace = new Workspace(command.Name, command.Address);
        dbContext.Workspaces.Add(workspace);

        await dbContext.SaveChangesAsync(ct);

        return workspace.ToDto();
    }
}
