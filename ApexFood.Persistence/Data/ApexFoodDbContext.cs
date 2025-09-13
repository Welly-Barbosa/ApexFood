// ApexFood.Persistence/Data/ApexFoodDbContext.cs

using ApexFood.Application.Common.Interfaces; 
using ApexFood.Domain.Common; 
using ApexFood.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ApexFood.Persistence.Extensions;

namespace ApexFood.Persistence.Data;

public class ApexFoodDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ITenantResolver _tenantResolver;

    public ApexFoodDbContext(
        DbContextOptions<ApexFoodDbContext> options,
        ITenantResolver tenantResolver) // Injetar o resolvedor
        : base(options)
    {
        _tenantResolver = tenantResolver;
    }

    /// <summary>
    /// Obtém ou define o conjunto de entidades para Tenants.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>
    /// Obtém ou define o conjunto de entidades para Insumos.
    /// </summary>
    public DbSet<Insumo> Insumos { get; set; }

    /// <summary>
    /// Obtém ou define o conjunto de entidades para Users.
    /// </summary>
    public override DbSet<User> Users { get; set; }

    /// <summary>
    /// Obtém ou define o conjunto de entidades para Roles.
    /// </summary>
    public override DbSet<IdentityRole<Guid>> Roles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ==================================================================
        // PASSO NOVO: Adiciona o Filtro Global de Multi-Tenancy
        // ==================================================================
        modelBuilder.AppendTenantQueryFilter(_tenantResolver);
    }
}