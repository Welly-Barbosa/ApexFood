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
    // Atualmente, não precisamos de propriedades personalizadas, mas esta classe
    // nos dá a flexibilidade de adicioná-las no futuro.
    // Por exemplo:
    // public string? FullName { get; set; }
    // public DateTime DateOfBirth { get; set; }
}