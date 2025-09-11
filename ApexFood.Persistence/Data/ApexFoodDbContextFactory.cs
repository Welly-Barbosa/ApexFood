// ApexFood.Persistence/Data/ApexFoodDbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ApexFood.Persistence.Data;

/// <summary>
/// Fábrica para criar instâncias de ApexFoodDbContext em tempo de design (ex: para criar migrações).
/// Esta classe permite que as ferramentas do EF Core criem o DbContext sem precisar
/// executar o projeto de inicialização da API, contornando problemas de configuração de host.
/// </summary>
public class ApexFoodDbContextFactory : IDesignTimeDbContextFactory<ApexFoodDbContext>
{
    /// <summary>
    /// Cria uma nova instância do DbContext.
    /// </summary>
    /// <param name="args">Argumentos passados pela linha de comando (não utilizados aqui).</param>
    /// <returns>Uma instância de ApexFoodDbContext.</returns>
    public ApexFoodDbContext CreateDbContext(string[] args)
    {
        // Constrói um objeto de configuração para ler o appsettings.json
        // A busca pelo arquivo sobe a partir do diretório do projeto de persistência
        // até encontrar a raiz da solução e, então, o projeto da API.
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ApexFood.Api"))
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApexFoodDbContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new ApexFoodDbContext(optionsBuilder.Options);
    }
}