// ApexFood.Api.IntegrationTests/IntegrationTestWebAppFactory.cs
using ApexFood.Persistence.Data;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
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
    private readonly MsSqlContainer _dbContainer;

    public IntegrationTestWebAppFactory()
    {
        // Configura o container do SQL Server com senha forte, EULA aceita e timeout maior
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Str0ng_Passw0rd_123!")
            .WithEnvironment("ACCEPT_EULA", "Y")
            // Aumenta timeout de inicialização para até 2 minutos
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted(
                    "/opt/mssql-tools/bin/sqlcmd",
                    "-S", "localhost",
                    "-U", "sa",
                    "-P", "Str0ng_Passw0rd_123!",
                    "-Q", "SELECT 1")
                .WithStartupTimeout(TimeSpan.FromMinutes(2)))
            .Build();
    }

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
