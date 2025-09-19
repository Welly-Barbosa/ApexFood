// ApexFood.Domain/Common/ITenantEntity.cs
namespace ApexFood.Domain.Common;

/// <summary>
/// Define uma entidade que pertence a um Tenant.
/// Todas as entidades que implementarem esta interface serão automaticamente
/// filtradas pelo Global Query Filter de Multi-Tenancy.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Obtém ou define o ID do Tenant ao qual a entidade pertence.
    /// </summary>
    public Guid TenantId { get; set; }
}