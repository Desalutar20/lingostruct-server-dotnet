using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users.Dto;

public sealed record AdminUserDto
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Role { get; init; } = null!;
    public string? AvatarUrl { get; init; }
    public bool IsVerified { get; init; }
    public bool IsBanned { get; init; }
    public string? GoogleId { get; init; }
    public string? GithubId { get; init; }
}

internal static class AdminUserDtoMapper
{
    public static AdminUserDto ToDto(this User user) =>
        new()
        {
            Id = user.Id.Value,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Email = user.Email.Value,
            FirstName = user.FirstName?.Value,
            LastName = user.LastName?.Value,
            Role = user.Role.Value,
            AvatarUrl = user.AvatarUrl?.Value,
            IsVerified = user.IsVerified,
            IsBanned = user.IsBanned,
            GoogleId = user.GoogleId?.Value,
            GithubId = user.GithubId?.Value,
        };
}
