// ApexFood.Api/Contracts/Insumos/CriarInsumoRequest.cs
namespace ApexFood.Api.Contracts.Insumos;

public record CriarInsumoRequest(
    string Nome,
    string UnidadeMedidaBase,
    string? Gtin,
    string? Sku);