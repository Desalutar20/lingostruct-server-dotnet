namespace src.Modules.Shared.Domain;

public abstract class Entity<T>(T id) : IEntity
{
    public T Id { get; init; } = id;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
