namespace GjejVend.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public class DomainException(string message) : Exception(message);
