using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Admin.Workspaces.UseCases.Queries.GetAdminWorkspaces;

internal sealed record WorkspacesFilters(NonEmptyString? Search);
