// ApexFood.Application/Features/Insumos/CriarInsumoCommand.cs
using MediatR;
namespace ApexFood.Application.Features.Insumos;

public record CriarInsumoCommand(
    string Nome,
    string UnidadeMedidaBase,
    string? Gtin,
    string? Sku) : IRequest<Guid>;