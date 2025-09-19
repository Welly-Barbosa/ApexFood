// ApexFood.Domain/Entities/User.cs

using Microsoft.AspNetCore.Identity;

namespace ApexFood.Domain.Entities;

/// <summary>
/// Representa um usuário no sistema.
/// Herda de IdentityUser para integrar-se ao sistema de associação do ASP.NET Core Identity.
/// Usamos Guid como o tipo da chave primária para consistência em todo o domínio.
/// </summary>
public class User : IdentityUser<Guid>
{
    // ==================================================================
    // PROPRIEDADES ADICIONADAS PARA MULTI-TENANCY
    // ==================================================================

    /// <summary>
    /// Chave estrangeira para o Tenant ao qual este usuário pertence.
    /// Pode ser nulo para usuários de nível de sistema (ex: super admin).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Propriedade de navegação para o Tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }
}