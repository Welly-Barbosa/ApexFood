// ApexFood.Domain/Common/Entity.cs

namespace ApexFood.Domain.Common;

/// <summary>
/// Classe base para entidades que são raízes de agregado e utilizam um Guid como identificador.
/// Herda de AggregateRoot e padroniza o tipo da chave primária.
/// </summary>
public abstract class Entity : AggregateRoot<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class with a new Guid.
    /// </summary>
    protected Entity() : base(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class with a specific Guid.
    /// </summary>
    /// <param name="id">O identificador único da entidade.</param>
    protected Entity(Guid id) : base(id)
    {
    }
}