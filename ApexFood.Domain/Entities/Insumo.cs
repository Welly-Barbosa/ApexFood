// ApexFood.Domain/Entities/Insumo.cs
using ApexFood.Domain.Common;

namespace ApexFood.Domain.Entities;

public class Insumo : Entity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Nome { get; private set; }
    public string UnidadeMedidaBase { get; private set; }
    public string? Gtin { get; private set; } // Código de Barras
    public string? Sku { get; private set; }  // Código Interno
    public bool IsAtivo { get; private set; } = true;

#pragma warning disable CS8618 // Construtor para o EF Core
    private Insumo() { }
#pragma warning restore CS8618

    public Insumo(Guid tenantId, string nome, string unidadeMedidaBase, string? gtin = null, string? sku = null) : base()
    {
        TenantId = tenantId;
        Nome = nome;
        UnidadeMedidaBase = unidadeMedidaBase;
        Gtin = gtin;
        Sku = sku;
        IsAtivo = true; // Um novo insumo começa como ativo por padrão
    }

    public void Desativar() => IsAtivo = false;
    public void Ativar() => IsAtivo = true;
}