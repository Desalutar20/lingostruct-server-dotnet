using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Domain.Pagination;

internal sealed record OffsetPagination
{
    public OffsetPagination(PositiveInt limit, PositiveInt? page)
    {
        var defaultPage = PositiveInt.Create(1).Value;

        Limit = limit;
        Page = page ?? defaultPage;
    }

    public PositiveInt Limit { get; }
    public PositiveInt Page { get; }

    public int Offset => (Page.Value - 1) * Limit.Value;
}
