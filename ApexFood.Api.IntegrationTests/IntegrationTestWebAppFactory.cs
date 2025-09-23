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
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_password_123!")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // A linha mais importante: diz ao host da aplicação para usar o ambiente "Testing".
        // Isso ativará a lógica condicional que acabamos de colocar no Program.cs.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // A factory agora tem uma única responsabilidade: substituir a conexão do banco de dados.
            services.RemoveAll(typeof(DbContextOptions<ApexFoodDbContext>));
            services.AddDbContext<ApexFoodDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
        });

        // A seção ConfigureLogging foi removida, pois não é mais necessária.
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.StopAsync();
}