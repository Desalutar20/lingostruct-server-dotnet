namespace src.Modules.Workspaces.Domain;

using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

public sealed record WorkspaceName : NonEmptyString
{
    public const int MaxLength = 100;

    private WorkspaceName(string value)
        : base(value) { }

    public static Result<WorkspaceName> Create(string value) =>
        NonEmptyString.Create<WorkspaceName>(
            value,
            "name",
            "Workspace name",
            MaxLength,
            (val) => new WorkspaceName(val)
        );

    public override string ToString() => base.ToString();
};
