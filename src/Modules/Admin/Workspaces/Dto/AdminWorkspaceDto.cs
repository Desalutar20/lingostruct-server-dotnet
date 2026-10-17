using src.Modules.Workspaces.Domain;

namespace src.Modules.Admin.Workspaces.Dto;

public sealed record AdminWorkspaceDto
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required string Name { get; init; }
    public string Country { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string StreetNumber { get; init; } = null!;
    public string PostalCode { get; init; } = null!;
}

internal static class AdminWorkspaceDtoMapper
{
    public static AdminWorkspaceDto ToDto(this Workspace workspace) =>
        new()
        {
            Id = workspace.Id.Value,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt,
            Name = workspace.Name.Value,
            Country = workspace.Address.Country,
            City = workspace.Address.City,
            Street = workspace.Address.Street,
            StreetNumber = workspace.Address.StreetNumber,
            PostalCode = workspace.Address.PostalCode,
        };
}
