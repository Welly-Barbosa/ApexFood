// ApexFood.Api/Program.cs

using ApexFood.Persistence.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up ApexFood.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // ==================================================================
    // AÇÃO 1.1: Adiciona o serviço de Health Checks robusto.
    // Isso define um health check 'liveness' que apenas confirma se a app está viva,
    // sem depender de bancos de dados ou outros serviços. É rápido e confiável.
    // ==================================================================
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("A aplicação está funcional."));

    // PASSO NOVO: Registra o ApexFoodDbContext no container de DI.
    builder.Services.AddDbContext<ApexFoodDbContext>(options =>
    {
        // Lê a string de conexão "DefaultConnection" do arquivo appsettings.json.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Configura o DbContext para usar o SQL Server com a string de conexão obtida.
        options.UseSqlServer(connectionString);
    });

    var app = builder.Build();

    // ==================================================================
    // AÇÃO 1.2: Corrige o aviso de redirecionamento HTTPS em ambientes de proxy.
    // Isso informa à aplicação para confiar nos cabeçalhos X-Forwarded-Proto
    // enviados pelo balanceador de carga do Azure.
    // ==================================================================
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseSerilogRequestLogging();

    // Opcional: Se ainda estiver usando UseHttpsRedirection(), agora ele funcionará sem warnings.
    app.UseHttpsRedirection();

    app.MapGet("/", () => "ApexFood API is running!");

    // ==================================================================
    // AÇÃO 1.3: Mapeia o endpoint de Health Checks para a rota /health
    // com um formato de resposta JSON detalhado.
    // ==================================================================
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var options = new JsonSerializerOptions { WriteIndented = true };
            var payload = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration
                })
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}