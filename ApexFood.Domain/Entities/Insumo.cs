// ApexFood.Domain/Entities/Insumo.cs
using ApexFood.Domain.Common;

namespace ApexFood.Domain.Entities;

public class Insumo : Entity, ITenantEntity
{
    public string Name { get; private set; }
    public Guid TenantId { get; set; }

    // Construtor privado para o EF Core.
    // O #pragma desabilita o aviso CS8618 especificamente para esta linha.
#pragma warning disable CS8618
    private Insumo() { }
#pragma warning restore CS8618

    public Insumo(string name, Guid tenantId) : base()
    {
        Name = name;
        TenantId = tenantId;
    }
}