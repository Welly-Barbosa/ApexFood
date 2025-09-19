// ApexFood.Api.IntegrationTests/IntegrationTestWebAppFactory.cs
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
    private const string SaPassword = "Str0ng_Passw0rd_123!";
    private static readonly TimeSpan SqlStartupTimeout = TimeSpan.FromMinutes(3);

    private readonly MsSqlContainer _dbContainer;

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(SaPassword)
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
        Console.WriteLine("[IntegrationTestWebAppFactory] Starting SQL Server container...");
        await _dbContainer.StartAsync();

        Console.WriteLine("[IntegrationTestWebAppFactory] Container started. Beginning readiness loop...");

        var sw = Stopwatch.StartNew();
        var baseConnString = _dbContainer.GetConnectionString();

        // Define timeout de conexão via connection string builder
        var builder = new SqlConnectionStringBuilder(baseConnString)
        {
            ConnectTimeout = 5
        };
        var connStringWithTimeout = builder.ConnectionString;

        while (sw.Elapsed < SqlStartupTimeout)
        {
            try
            {
                await using var conn = new SqlConnection(connStringWithTimeout);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                var result = await cmd.ExecuteScalarAsync();

                if (result != null)
                {
                    Console.WriteLine($"[IntegrationTestWebAppFactory] SQL Server ready after {sw.Elapsed.TotalSeconds:N1}s.");
                    return;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message?.Length > 300 ? ex.Message[..300] + "..." : ex.Message;
                Console.WriteLine($"[IntegrationTestWebAppFactory] Readiness attempt failed: {msg}");
            }

            await Task.Delay(1000);
        }

        try
        {
            Console.WriteLine("[IntegrationTestWebAppFactory] Readiness timeout. Fetching container logs...");
            var logs = await _dbContainer.GetLogsAsync();
            Console.WriteLine("===== BEGIN SQL CONTAINER LOGS =====");
            Console.WriteLine(logs);
            Console.WriteLine("=====  END SQL CONTAINER LOGS  =====");
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"[IntegrationTestWebAppFactory] Failed to retrieve container logs: {logEx.Message}");
        }

        throw new TimeoutException($"SQL Server container did not become ready within {SqlStartupTimeout.TotalSeconds} seconds.");
    }

    public new async Task DisposeAsync()
    {
        try
        {
            Console.WriteLine("[IntegrationTestWebAppFactory] Stopping SQL Server container...");
            await _dbContainer.StopAsync();
            Console.WriteLine("[IntegrationTestWebAppFactory] Container stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IntegrationTestWebAppFactory] Error stopping container: {ex.Message}");
        }
    }
}
