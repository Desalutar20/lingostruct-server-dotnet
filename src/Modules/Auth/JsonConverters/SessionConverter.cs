using System.Text.Json;
using System.Text.Json.Serialization;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

internal sealed class SessionConverter : JsonConverter<Session>
{
    public override Session Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var id = new UserId(Guid.Parse(root.GetProperty("userId").GetString()!));
        var email = Email.Create(root.GetProperty("email").GetString()!).Value;
        var firstName = root.TryGetProperty("firstName", out var fn)
            ? FirstName.Create(fn.GetString()!).Value
            : null;
        var lastName = root.TryGetProperty("lastName", out var ln)
            ? LastName.Create(ln.GetString()!).Value
            : null;
        var role = UserRole.Create(root.GetProperty("role").GetString()!).Value;
        var avatarUrl = root.TryGetProperty("avatarUrl", out var au)
            ? Url.Create(au.GetString()!).Value
            : null;

        return new Session(id, email, firstName, lastName, role, avatarUrl);
    }

    public override void Write(Utf8JsonWriter writer, Session value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.UserId.Value);
        writer.WriteString("email", value.Email.Value);

        if (value.FirstName is not null)
            writer.WriteString("firstName", value.FirstName.Value);

        if (value.LastName is not null)
            writer.WriteString("lastName", value.LastName.Value);

        writer.WriteString("role", value.Role.Value);

        if (value.AvatarUrl is not null)
        {
            writer.WriteString("avatarUrl", value.AvatarUrl.Value);
        }

        writer.WriteEndObject();
    }
}
