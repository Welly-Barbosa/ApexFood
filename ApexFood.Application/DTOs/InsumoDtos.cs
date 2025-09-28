using System;

namespace ApexFood.Application.DTOs;

/// <summary>
/// DTO para a resposta da listagem e detalhamento de insumos.
/// </summary>
public record InsumoResponseDto(
    Guid Id,
    string Nome,
    string UnidadeMedida,
    /// <summary>
    /// Identifica a origem do insumo.
    /// "Rede": Insumo homologado pelo franqueador.
    /// "Local": Insumo cadastrado pelo próprio franqueado.
    /// </summary>
    string Origem,
    /// <summary>
    /// Status do insumo. Insumos com 'false' são considerados desativados (soft-delete).
    /// </summary>
    bool IsAtivo
);

/// <summary>
/// DTO para a criação de um novo insumo.
/// </summary>
public record InsumoCreateDto(
    string Nome,
    string UnidadeMedida
// O 'Origem' e 'FranchiseId' serão definidos no backend com base no usuário autenticado.
);

/// <summary>
/// DTO para a atualização de um insumo existente.
/// </summary>
public record InsumoUpdateDto(
    string Nome,
    string UnidadeMedida
);

/// <summary>
/// DTO para a criação de um novo registro no histórico de preços de um insumo.
/// </summary>
public record HistoricoPrecoCreateDto(
    Guid InsumoId,
    decimal Preco,
    /// <summary>
    /// Data em que o preço se torna vigente. O formato esperado é 'YYYY-MM-DD'.
    /// </summary>
    DateOnly DataVigencia
);

/// <summary>
/// DTO para a resposta do histórico de preços de um insumo.
/// </summary>
public record HistoricoPrecoResponseDto(
    Guid Id,
    decimal Preco,
    DateOnly DataVigencia
);