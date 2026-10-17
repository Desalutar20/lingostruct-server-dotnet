using Bogus;
using src.Modules.Admin.Workspaces.Endpoints;
using src.Modules.Auth.Endpoints;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;
using src.Modules.Workspaces.Domain;

namespace Integration;

public static class TestData
{
    private static readonly Faker Faker = new();

    public static SignUpRequest SignUpRequest() =>
        new(FakeEmail(), FakePassword(), Faker.Name.FirstName(), Faker.Name.LastName());

    public static CreateWorkspaceRequest CreateWorkspaceRequest() =>
        new(
            Faker.Random.AlphaNumeric(WorkspaceName.MaxLength),
            Faker.Random.AlphaNumeric(WorkspaceAddress.CountryMaxLength),
            Faker.Random.AlphaNumeric(WorkspaceAddress.CityMaxLength),
            Faker.Random.AlphaNumeric(WorkspaceAddress.StreetMaxLength),
            Faker.Random.AlphaNumeric(WorkspaceAddress.StreetNumberMaxLength),
            Faker.Random.AlphaNumeric(WorkspaceAddress.PostalCodeMaxLength)
        );

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

                return new User(email, hashedPassword, null, null);
            })
            .ToList();

    public static List<Workspace> SeedWorkspaces() =>
        Enumerable
            .Range(1, 200)
            .Select(i =>
            {
                var name = WorkspaceName
                    .Create(Faker.Random.AlphaNumeric(WorkspaceName.MaxLength))
                    .Value;
                var address = WorkspaceAddress
                    .Create(
                        Faker.Random.AlphaNumeric(WorkspaceAddress.CountryMaxLength),
                        Faker.Random.AlphaNumeric(WorkspaceAddress.CityMaxLength),
                        Faker.Random.AlphaNumeric(WorkspaceAddress.StreetMaxLength),
                        Faker.Random.AlphaNumeric(WorkspaceAddress.StreetNumberMaxLength),
                        Faker.Random.AlphaNumeric(WorkspaceAddress.PostalCodeMaxLength)
                    )
                    .Value;

                return new Workspace(name, address);
            })
            .ToList();
}
