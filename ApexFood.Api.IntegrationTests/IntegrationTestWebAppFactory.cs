// ApexFood.Api.IntegrationTests/IntegrationTestWebAppFactory.cs
using ApexFood.Persistence.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace ApexFood.Api.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ==================================================================
    // SEÇÃO CORRIGIDA: Configura o contêiner com os parâmetros obrigatórios.
    // ==================================================================
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        // 1. Fornece uma senha forte para o usuário 'sa'
        .WithPassword("Strong_password_123!")
        // 2. Aceita os termos de licença (EULA), que é obrigatório
        .WithEnvironment("ACCEPT_EULA", "Y")
        .Build();



    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApexFoodDbContext>));
            services.AddDbContext<ApexFoodDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.StopAsync();
}