using src.Modules.Shared.Domain;

namespace src.Modules.Shared.CQRS;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct = default);
}
