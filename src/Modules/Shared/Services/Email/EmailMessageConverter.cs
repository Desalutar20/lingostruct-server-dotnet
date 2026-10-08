using System.Text.Json;
using System.Text.Json.Serialization;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Services.Email;

public sealed class EmailMessageConverter : JsonConverter<EmailMessage>
{
    public override EmailMessage Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;

        var subject = NonEmptyString.Create(root.GetProperty("subject").GetString()!).Value;
        var email = Domain.ValueObjects.Email.Create(root.GetProperty("to").GetString()!).Value;
        var plainText = NonEmptyString.Create(root.GetProperty("plainText").GetString()!).Value;

        var htmlText = root.TryGetProperty("htmlText", out var fn)
            ? NonEmptyString.Create(fn.GetString()!).Value
            : null;

        return new EmailMessage(subject, email, plainText, htmlText);
    }

    public override void Write(
        Utf8JsonWriter writer,
        EmailMessage value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WriteString("subject", value.Subject.Value);
        writer.WriteString("to", value.To.Value);
        writer.WriteString("plainText", value.PlainText.ToString());

        if (value.HtmlText is not null)
            writer.WriteString("htmlText", value.HtmlText.Value);

        writer.WriteEndObject();
    }
}
