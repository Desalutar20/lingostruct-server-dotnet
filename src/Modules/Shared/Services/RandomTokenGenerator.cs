using System.Security.Cryptography;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Services;

internal static class RandomTokenGenerator
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static Result<NonEmptyString> Generate(int length = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var result = new char[length];

        for (var i = 0; i < length; i++)
            result[i] = Chars[bytes[i] % Chars.Length];

        return NonEmptyString.Create(new string(result));
    }
}
