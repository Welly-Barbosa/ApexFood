// Localização: src/ApexFood.Api/Services/InsumoDataStore.cs
using ApexFood.Application.DTOs;

namespace ApexFood.Api.Services;

/// <summary>
/// Serviço Singleton para simular o armazenamento de insumos em memória.
/// </summary>
public class InsumoDataStore
{
    private readonly List<InsumoResponseDto> _insumos = new()
    {
        new InsumoResponseDto(Guid.NewGuid(), "Farinha de Trigo", "Kg", "Rede", true),
        new InsumoResponseDto(Guid.NewGuid(), "Tomate Italiano", "Kg", "Rede", true),
        new InsumoResponseDto(Guid.NewGuid(), "Manjericão Fresco", "Maço", "Local", true),
        new InsumoResponseDto(Guid.NewGuid(), "Queijo Parmesão (Antigo)", "Kg", "Rede", false)
    };

    public IEnumerable<InsumoResponseDto> GetAll(bool incluirInativos)
    {
        if (incluirInativos)
        {
            return _insumos;
        }
        return _insumos.Where(i => i.IsAtivo);
    }

    public InsumoResponseDto Add(InsumoCreateDto novoInsumoDto)
    {
        var insumo = new InsumoResponseDto(
            Guid.NewGuid(),
            novoInsumoDto.Nome,
            novoInsumoDto.UnidadeMedida,
            "Local",
            true
        );
        _insumos.Add(insumo);
        return insumo;
    }
}