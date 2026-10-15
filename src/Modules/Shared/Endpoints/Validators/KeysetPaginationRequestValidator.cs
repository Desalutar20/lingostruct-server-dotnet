using FluentValidation;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.Pagination;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;

namespace src.Modules.Shared.Endpoints.Validators;

public class KeysetPaginationRequestValidator<T> : AbstractValidator<T>
    where T : KeysetPaginationRequest
{
    public KeysetPaginationRequestValidator(int maxLimit, int cursorMaxLength)
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, maxLimit);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<Guid>.Create(
                    x,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid id") : id
                )
            )
            .MaximumLength(cursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<Guid>.Create(
                    x,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid id") : id
                )
            )
            .MaximumLength(cursorMaxLength);
    }
}
