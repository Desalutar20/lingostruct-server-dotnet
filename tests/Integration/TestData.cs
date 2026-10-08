using Bogus;
using src.Modules.Auth.Endpoints;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace Integration;

public static class TestData
{
    private static readonly Faker Faker = new();

    public static SignUpRequest SignUpRequest() =>
        new Faker<SignUpRequest>()
            .CustomInstantiator(f => new SignUpRequest(
                FakeEmail(),
                FakePassword(),
                f.Name.FirstName(),
                f.Name.LastName()
            ))
            .Generate();

    public static string FakeEmail() => Faker.Internet.Email();

    public static string String(int length) => Faker.Commerce.Random.AlphaNumeric(length);

    public static string FakePassword(int length = Password.MinLength) =>
        Faker.Internet.Password(length);

    public static List<User> SeedUsers() =>
        Enumerable
            .Range(1, 200)
            .Select(i =>
            {
                var email = Email.Create($"user{i}@test.local").Value;
                var hashedPassword = HashedPassword
                    .Create(FakePassword(HashedPassword.MinLength))
                    .Value;

                return new User(null, null, email, hashedPassword);
            })
            .ToList();
}
