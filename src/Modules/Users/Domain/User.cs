using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed class User(
    Email email,
    HashedPassword? hashedPassword,
    FirstName? firstName,
    LastName? lastName
) : Entity<UserId>(new UserId(Guid.NewGuid()))
{
    public FirstName? FirstName { get; private set; } = firstName;
    public LastName? LastName { get; private set; } = lastName;
    public Email Email { get; private set; } = email;
    public HashedPassword? HashedPassword { get; private set; } = hashedPassword;
    public UserRole Role { get; private set; } = UserRole.Regular;
    public bool IsBanned { get; private set; }
    public bool IsVerified { get; private set; }
    public Url? AvatarUrl { get; private set; }
    public ProviderId? GoogleId { get; private set; }
    public ProviderId? GithubId { get; private set; }

    public bool IsValid => !IsBanned && IsVerified;

    public Result Verify()
    {
        if (IsBanned)
        {
            return Error.Failure("User is banned");
        }

        IsVerified = true;
        return Result.Success();
    }

    public void UpdatePassword(HashedPassword newPassword) => HashedPassword = newPassword;

    public Result LinkOAuthProvider(OAuthProvider provider, ProviderId providerId)
    {
        if (provider == OAuthProvider.Google)
        {
            if (GoogleId is not null)
                return Result.Success();

            GoogleId = providerId;
            return Result.Success();
        }

        if (provider == OAuthProvider.Github)
        {
            if (GithubId is not null)
                return Result.Success();

            GithubId = providerId;
            return Result.Success();
        }

        return Error.Failure($"Unsupported OAuth provider: {provider.Value}");
    }

    public bool Update(
        Optional<FirstName?> firstName,
        Optional<LastName?> lastName,
        Optional<Url?> avatarUrl,
        Optional<bool> isBanned
    )
    {
        var isUpdated = false;

        if (firstName.HasValue && FirstName != firstName.Value)
        {
            FirstName = firstName.Value;
            isUpdated = true;
        }

        if (lastName.HasValue && LastName != lastName.Value)
        {
            LastName = lastName.Value;
            isUpdated = true;
        }

        if (avatarUrl.HasValue && AvatarUrl != avatarUrl.Value)
        {
            AvatarUrl = avatarUrl.Value;
            isUpdated = true;
        }

        if (isBanned.HasValue && IsBanned != isBanned.Value)
        {
            IsBanned = isBanned.Value;
            isUpdated = true;
        }

        return isUpdated;
    }
}
