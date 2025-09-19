// ApexFood.Domain/Entities/ProdutoModelo.cs
using ApexFood.Domain.Common;
using ApexFood.Domain.Enums; // <-- ADICIONE ESTE USING

namespace ApexFood.Domain.Entities;

/// <summary>
/// Representa o "blueprint" de um produto, servindo como modelo para suas variações vendáveis.
/// </summary>
public class ProdutoModelo : Entity
{
    /// <summary>
    /// Identifica o proprietário do registro (franqueador ou franqueado).
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nome do produto (ex: "Pizza Calabresa").
    /// </summary>
    public string Nome { get; private set; }

    /// <summary>
    /// Descrição para o cardápio.
    /// </summary>
    public string? Descricao { get; private set; }

    /// <summary>
    /// Agrupador para relatórios (ex: "Pizzas", "Hambúrgueres").
    /// </summary>
    public string? Categoria { get; private set; }

    /// <summary>
    /// Flag que indica se o modelo de produto está ativo.
    /// </summary>
    public bool IsAtivo { get; private set; } = true;

    // ==================================================================
    // PROPRIEDADES ADICIONADAS PARA O NOVO CICLO DE VIDA
    // ==================================================================

    /// <summary>
    /// O estágio atual do produto no fluxo de cadastro.
    /// O valor padrão é pendente de ficha técnica.
    /// </summary>
    public StatusProduto Status { get; private set; } = StatusProduto.PendenteFichaTecnica;

    /// <summary>
    /// Controla a visibilidade do produto para a rede de franqueados.
    /// Relevante apenas para produtos cujo TenantId seja de um franqueador.
    /// </summary>
    public bool IsPublicadoParaRede { get; private set; } = false;

#pragma warning disable CS8618 // Construtor privado para o EF Core
    private ProdutoModelo() { }
#pragma warning restore CS8618

    public ProdutoModelo(Guid tenantId, string nome, string? descricao, string? categoria) : base()
    {
        TenantId = tenantId;
        Nome = nome; // Validações podem ser adicionadas aqui
        Descricao = descricao;
        Categoria = categoria;
    }

    // Métodos para gerenciar o estado do produto
    public void PublicarParaRede() => IsPublicadoParaRede = true;
    public void DespublicarDaRede() => IsPublicadoParaRede = false;
    public void AtualizarStatus(StatusProduto novoStatus) => Status = novoStatus;
    public void Desativar() => IsAtivo = false;
}