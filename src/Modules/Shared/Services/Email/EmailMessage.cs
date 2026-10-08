using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Services.Email;

public record struct EmailMessage(
    NonEmptyString Subject,
    Shared.Domain.ValueObjects.Email To,
    NonEmptyString PlainText,
    NonEmptyString? HtmlText
);
