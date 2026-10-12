public sealed record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string Role,
    string? AvatarUrl
);

internal static class UserDtoMapper
{
    public static UserDto ToDto(this Session user) =>
        new(
            user.UserId.Value,
            user.Email.Value,
            user.FirstName?.Value,
            user.LastName?.Value,
            user.Role.Value,
            user.AvatarUrl?.Value
        );
}
