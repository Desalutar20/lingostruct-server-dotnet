namespace src.Modules.Shared.Domain.ValueObjects;

public record Id(Guid Value)
{
    public override string ToString() => Value.ToString();
}
