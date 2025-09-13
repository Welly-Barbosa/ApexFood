// ApexFood.Persistence/Data/ApexFoodDbContextFactory.cs

using ApexFood.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ApexFood.Persistence.Data;

public class ApexFoodDbContextFactory : IDesignTimeDbContextFactory<ApexFoodDbContext>
{
    public ApexFoodDbContext CreateDbContext(string[] args)
    {
        // A lógica para encontrar o appsettings.json permanece a mesma.
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ApexFood.Api"))
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApexFoodDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);

        // ==================================================================
        // SOLUÇÃO: Criamos um resolvedor de tenant "falso" que não faz nada,
        // apenas o suficiente para que o DbContext possa ser construído.
        // ==================================================================
        var dummyTenantResolver = new DesignTimeTenantResolver();

        return new ApexFoodDbContext(optionsBuilder.Options, dummyTenantResolver);
    }

    /// <summary>
    /// Implementação stub de ITenantResolver usada apenas em tempo de design.
    /// </summary>
    private class DesignTimeTenantResolver : ITenantResolver
    {
        public Guid GetTenantId() => Guid.Empty; // Retorna um valor padrão que não afeta a criação da migração.
    }
}