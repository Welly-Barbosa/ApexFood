// ApexFood.Domain/Enums/PlanoAssinatura.cs

namespace ApexFood.Domain.Enums;

/// <summary>
/// Define os diferentes níveis de planos de assinatura disponíveis na plataforma.
/// </summary>
public enum PlanoAssinatura
{
    /// <summary>
    /// Plano de entrada com funcionalidades essenciais.
    /// </summary>
    Basico = 1,

    /// <summary>
    /// Plano intermediário com ferramentas de eficiência e análise.
    /// </summary>
    Intermediario = 2,

    /// <summary>
    /// Plano avançado com automação e inteligência de negócio.
    /// </summary>
    Pro = 3,

    /// <summary>
    /// Plano completo para redes de franquias com gestão hierárquica.
    /// </summary>
    Enterprise = 4
}