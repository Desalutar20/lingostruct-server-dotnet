using System.Security.Cryptography;
using System.Text;
using src.Modules.Shared.Domain;
using src.Modules.Users.Domain;

namespace src.Modules.Shared.Services;

public static class HashingService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static Result<HashedPassword> Hash(Password password)
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password.Value),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return HashedPassword.Create(Convert.ToBase64String(hashBytes));
    }

    public static bool Verify(Password password, HashedPassword hashedPassword)
    {
        var hashBytes = Convert.FromBase64String(hashedPassword.Value);

        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password.Value),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        return CryptographicOperations.FixedTimeEquals(hashBytes.AsSpan(SaltSize), hash);
    }
}
