// ApexFood.Persistence/Data/ApexFoodDbContext.cs
using ApexFood.Application.Common.Interfaces;
using ApexFood.Domain.Common;
using ApexFood.Domain.Entities;
using ApexFood.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ApexFood.Persistence.Data;

public class ApexFoodDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ITenantResolver _tenantResolver;

    // ==================================================================
    // PROPRIEDADE ADICIONADA: Armazena o TenantId para a instância atual do DbContext.
    // ==================================================================
    public Guid TenantId { get; }

    public ApexFoodDbContext(
        DbContextOptions<ApexFoodDbContext> options,
        ITenantResolver tenantResolver)
        : base(options)
    {
        _tenantResolver = tenantResolver;
        // O valor do TenantId é definido uma vez por instância do DbContext.
        TenantId = _tenantResolver.GetTenantId();
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Insumo> Insumos { get; set; }
    public DbSet<ProdutoModelo> ProdutosModelo { get; set; }
    public override DbSet<User> Users { get; set; }
    public override DbSet<IdentityRole<Guid>> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Passamos a referência da própria instância do DbContext para o filtro.
        modelBuilder.AppendTenantQueryFilter(this);
    }
}