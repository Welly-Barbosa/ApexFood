// ApexFood.Api.IntegrationTests/IntegrationTestWebAppFactory.cs
using ApexFood.Persistence.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace ApexFood.Api.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_password_123!")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Define o ambiente para "Testing" para que possamos, se necessário,
        // ter configurações diferentes para testes no futuro.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove a configuração do DbContext original
            services.RemoveAll(typeof(DbContextOptions<ApexFoodDbContext>));

            // Adiciona uma nova configuração do DbContext que aponta para o banco de teste
            services.AddDbContext<ApexFoodDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
        });

        // ==================================================================
        // PASSO CORRIGIDO: Sobrescreve a configuração de logging para os testes.
        // Isso impede que o Serilog seja inicializado e cause o erro "logger is frozen".
        // ==================================================================
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders(); // Remove o Serilog e outros providers
            logging.AddConsole();     // Adiciona um logger de console simples para vermos os logs do teste
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.StopAsync();
}