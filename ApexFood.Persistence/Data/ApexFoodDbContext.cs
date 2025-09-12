// ApexFood.Persistence/Data/ApexFoodDbContext.cs

using ApexFood.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ApexFood.Persistence.Data;

public class ApexFoodDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApexFoodDbContext(DbContextOptions<ApexFoodDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Obtém ou define o conjunto de entidades para Tenants.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; }

    // ==================================================================
    // DECLARAÇÕES EXPLÍCITAS PARA GARANTIR A DESCOBERTA PELA FERRAMENTA DE MIGRAÇÃO
    // ==================================================================

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
    }
}