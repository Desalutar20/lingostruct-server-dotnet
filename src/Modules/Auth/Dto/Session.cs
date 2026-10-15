using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

internal sealed record Session(
    UserId UserId,
    Email Email,
    FirstName? FirstName,
    LastName? LastName,
    UserRole Role,
    Url? AvatarUrl
)
{
    public static Session FromUser(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role, user.AvatarUrl);
};
