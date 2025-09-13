// ApexFood.Application/Common/Interfaces/IApplicationDbContext.cs
using ApexFood.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ApexFood.Application.Common.Interfaces;

/// <summary>
/// Define o contrato para o DbContext da aplicação, expondo os DbSets
/// necessários para a camada de aplicação.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Obtém o DbSet para a entidade Tenant.
    /// </summary>
    DbSet<Tenant> Tenants { get; }

    DbSet<Insumo> Insumos { get; }

    /// <summary>
    /// Salva todas as alterações feitas no contexto para o banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Um token para observar enquanto espera a tarefa ser concluída.</param>
    /// <returns>Uma tarefa que representa a operação de salvamento assíncrona.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}