namespace src.Modules.Workspaces.Domain;

using src.Modules.Shared.Domain;

public sealed class Workspace : Entity<WorkspaceId>
{
    private Workspace()
        : base(new WorkspaceId(Guid.Empty)) { }

    public Workspace(WorkspaceName name, WorkspaceAddress address)
        : base(new WorkspaceId(Guid.NewGuid()))
    {
        Name = name;
        Address = address;
    }

    public WorkspaceName Name { get; private set; } = null!;
    public WorkspaceAddress Address { get; private set; } = null!;

    public bool Update(WorkspaceName? name, WorkspaceAddress? address)
    {
        var isUpdated = false;

        if (name is not null && Name != name)
        {
            Name = name;
            isUpdated = true;
        }

        if (address is not null && Address != address)
        {
            Address = address;
            isUpdated = true;
        }

        return isUpdated;
    }
}
