using src.Modules.Shared.Domain.ValueObjects;

public sealed record UsersFilters(NonEmptyString? Search, bool? IsBanned, bool? IsVerified);
