namespace src.Modules.Workspaces.Domain;

using src.Modules.Shared.Domain.ValueObjects;

public sealed record WorkspaceId(Guid Value) : Id(Value)
{
    public override string ToString() => base.ToString();
};
