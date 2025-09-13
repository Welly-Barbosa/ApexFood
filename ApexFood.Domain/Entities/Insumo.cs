// ApexFood.Domain/Entities/Insumo.cs
using ApexFood.Domain.Common;

namespace ApexFood.Domain.Entities;

// A entidade Insumo agora implementa ITenantEntity
public class Insumo : Entity, ITenantEntity
{
    public string Name { get; private set; }
    public Guid TenantId { get; set; } // Propriedade exigida pela interface

    private Insumo() { }
    public Insumo(string name, Guid tenantId) : base()
    {
        Name = name;
        TenantId = tenantId;
    }
}