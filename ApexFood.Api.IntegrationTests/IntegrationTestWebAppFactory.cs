using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ApexFood.Persistence.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
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
        // Cria container sem o wait strategy padrão (que usa sqlcmd incorretamente)
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong_password_123!")
            .WithEnvironment("ACCEPT_EULA", "Y")
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

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Loop de readiness feito manualmente via SqlClient
        var timeout = TimeSpan.FromMinutes(2);
        var sw = Stopwatch.StartNew();
        var connString = _dbContainer.GetConnectionString();

        while (sw.Elapsed < timeout)
        {
            try
            {
                await using var conn = new SqlConnection(connString);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync();

                Console.WriteLine("[IntegrationTestWebAppFactory] SQL Server pronto para conexões.");
                return;
            }
            catch
            {
                // ainda não está pronto → espera e tenta de novo
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException("SQL Server container did not become ready within 120 seconds.");
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}
