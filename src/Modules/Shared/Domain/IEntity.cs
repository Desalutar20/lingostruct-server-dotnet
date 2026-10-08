namespace src.Modules.Shared.Domain;

public interface IEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}
