// ApexFood.Domain/Common/AggregateRoot.cs

namespace ApexFood.Domain.Common;

/// <summary>
/// Representa a raiz de um agregado no contexto do Domain-Driven Design.
/// Uma raiz de agregado é a entidade principal dentro de um agregado,
/// responsável por manter a consistência e as invariantes do negócio.
/// Garante que toda raiz de agregado possua um identificador único.
/// </summary>
/// <typeparam name="TId">O tipo do identificador do agregado.</typeparam>
public abstract class AggregateRoot<TId>
{
    /// <summary>
    /// Obtém ou define o identificador único do agregado.
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">O identificador para a raiz do agregado.</param>
    protected AggregateRoot(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// Necessário para ORMs como o Entity Framework Core.
    /// </summary>
#pragma warning disable CS8618 // Usado pelo EF Core, que irá popular a propriedade Id.
    protected AggregateRoot()
#pragma warning restore CS8618
    {
    }
}