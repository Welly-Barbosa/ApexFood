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
        // Mantive a sua abordagem: removo a configuração existente e registro DbContext apontando
        // para a connection string do container. A inicialização real do container acontece
        // em InitializeAsync (xUnit chamará InitializeAsync antes dos testes).
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApexFoodDbContext>));
            services.AddDbContext<ApexFoodDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
        });
    }

    // Inicializa o container e aguarda explicitamente até que seja possível conectar ao banco.
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Espera ativa (timeout total de 2 minutos). Faz tentativas de abrir conexão e executar SELECT 1.
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
                var scalar = await cmd.ExecuteScalarAsync();

                if (scalar != null)
                {
                    // Conseguiu executar SELECT 1 -> o SQL Server está pronto.
                    return;
                }
            }
            catch (Exception ex)
            {
                // Mostra logs curtos para ajudar no troubleshooting no CI.
                Console.WriteLine($"[IntegrationTestWebAppFactory] Ainda aguardando SQL Server... {(ex.Message.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message)}");
            }

            await Task.Delay(1000);
        }

        // Se estourou o timeout, captura logs do container (se disponível) e lança TimeoutException.
        try
        {
            var logs = await _dbContainer.GetLogsAsync();
            Console.WriteLine("[IntegrationTestWebAppFactory] Logs do container SQL Server:\n" + logs);
        }
        catch
        {
            // ignora falha ao obter logs
        }

        throw new TimeoutException($"SQL Server container did not become ready within {timeout.TotalSeconds} seconds.");
    }

    // Para o container ao final dos testes
    public new async Task DisposeAsync()
    {
        try
        {
            await _dbContainer.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IntegrationTestWebAppFactory] Erro ao parar container: {ex.Message}");
        }
    }
}
