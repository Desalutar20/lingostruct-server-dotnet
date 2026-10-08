namespace src.Modules.Shared.CQRS;

public interface ICommand<out TResponse>
    where TResponse : notnull;

public interface ICommand;
