// ApexFood.Persistence/Data/ApexFoodDbContext.cs

using ApexFood.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ApexFood.Persistence.Data;

/// <summary>
/// Representa a sessão com o banco de dados da aplicação, permitindo
/// a consulta e o salvamento de entidades.
/// </summary>
public class ApexFoodDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApexFoodDbContext"/> class.
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    public ApexFoodDbContext(DbContextOptions<ApexFoodDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Obtém ou define o conjunto de entidades para Tenants.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>
    /// Configura o modelo de dados para o contexto. A Fluent API é usada aqui
    /// para definir o schema do banco de dados.
    /// </summary>
    /// <param name="modelBuilder">O construtor que está sendo usado para o modelo.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de entidade que estão definidas
        // neste assembly (o projeto ApexFood.Persistence).
        // Isso descobre automaticamente classes como 'TenantConfiguration'.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}