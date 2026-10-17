namespace src.Modules.Workspaces.Domain;

using src.Modules.Shared.Domain;

public sealed record WorkspaceAddress
{
    public const int CountryMaxLength = 50;
    public const int CityMaxLength = 100;
    public const int StreetMaxLength = 100;
    public const int StreetNumberMaxLength = 50;
    public const int PostalCodeMaxLength = 20;

    public string Country { get; }
    public string City { get; }
    public string Street { get; }
    public string StreetNumber { get; }
    public string PostalCode { get; }

    private WorkspaceAddress(
        string country,
        string city,
        string street,
        string streetNumber,
        string postalCode
    )
    {
        Country = country;
        City = city;
        Street = street;
        StreetNumber = streetNumber;
        PostalCode = postalCode;
    }

    public static Result<WorkspaceAddress> Create(
        string country,
        string city,
        string street,
        string streetNumber,
        string postalCode
    )
    {
        var errors = new List<string>();

        Validate(ref country, CountryMaxLength, "Country", errors);
        Validate(ref city, CityMaxLength, "City", errors);
        Validate(ref street, StreetMaxLength, "Street", errors);
        Validate(ref streetNumber, StreetNumberMaxLength, "Street number", errors);
        Validate(ref postalCode, PostalCodeMaxLength, "Postal code", errors);

        if (errors.Count != 0)
        {
            return Error.Validation("address", errors);
        }

        return new WorkspaceAddress(country, city, street, streetNumber, postalCode);
    }

    private static void Validate(ref string value, int maxLength, string label, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{label} can not be empty");
        }
        else
        {
            value = value.Trim();
        }

        if (value.Length > maxLength)
        {
            errors.Add($"{label} can not be longer than {maxLength} characters");
        }
    }
}
