using src.Modules.Shared.Domain;

namespace src.Modules.Users.Domain.Exceptions;

public sealed class UserAlreadyExistsException(Exception innerException)
    : AppException("User already exists.", innerException);
