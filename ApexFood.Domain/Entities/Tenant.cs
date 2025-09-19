// ApexFood.Domain/Entities/Tenant.cs

using ApexFood.Domain.Common;

namespace ApexFood.Domain.Entities;

/// <summary>
/// Representa um Tenant no sistema, que pode ser uma franqueadora ou uma loja franqueada.
/// Esta entidade é uma raiz de agregado e define um limite de consistência para seus dados.
/// </summary>
public class Tenant : Entity
{
    /// <summary>
    /// Obtém ou define o nome do tenant (ex: "Matriz São Paulo", "Loja Shopping Morumbi").
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Obtém ou define o identificador do tenant pai.
    /// Nulo para tenants de nível superior (franqueadoras).
    /// Preenchido para tenants filhos (lojas franqueadas).
    /// </summary>
    public Guid? ParentTenantId { get; private set; }

    // Relação de navegação (opcional por enquanto, mas bom ter em mente)
    // public virtual Tenant? ParentTenant { get; private set; }
    // public virtual ICollection<Tenant> ChildTenants { get; private set; } = new List<Tenant>();

    /// <summary>
    /// Construtor privado para ser utilizado pelo Entity Framework Core.
    /// </summary>
#pragma warning disable CS8618 // Usado pelo EF Core, que irá popular as propriedades.
    private Tenant() : base()
#pragma warning restore CS8618
    {
    }

    /// <summary>
    /// Cria uma nova instância de um Tenant.
    /// </summary>
    /// <param name="name">O nome do tenant.</param>
    /// <param name="parentTenantId">O ID do tenant pai, se aplicável.</param>
    public Tenant(string name, Guid? parentTenantId = null) : base()
    {
        // TODO: Adicionar validações de negócio (ex: nome não pode ser nulo ou vazio).
        Name = name;
        ParentTenantId = parentTenantId;
    }
}