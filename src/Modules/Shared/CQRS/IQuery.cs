namespace src.Modules.Shared.CQRS;

public interface IQuery<out TResponse>
    where TResponse : notnull;
