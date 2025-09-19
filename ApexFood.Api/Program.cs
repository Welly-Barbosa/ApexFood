// ApexFood.Api/Program.cs

using ApexFood.Application.Common.Interfaces;
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Application.Common.Interfaces.Persistence;
using ApexFood.Application.Contracts.Authentication;
using ApexFood.Application.Features.Authentication;
using ApexFood.Domain.Entities;
using ApexFood.Infrastructure.Authentication;
using ApexFood.Infrastructure.Services;
using ApexFood.Persistence.Data;
using ApexFood.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up ApexFood.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Configuração de Logging (Serilog)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    // 2. Registro de Serviços no Container de DI

    // --- Serviços de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Serviços de Persistência (SEÇÃO CORRIGIDA) ---
    builder.Services.AddDbContext<ApexFoodDbContext>(options =>
    {
        // Lê a string de conexão do appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Configura o DbContext para usar o SQL Server com a string de conexão.
        options.UseSqlServer(connectionString);
    });
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());

    // ==================================================================
    // SEÇÃO ADICIONADA: Registra os Repositórios.
    // ==================================================================
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();

    // --- Serviços de Segurança e Autenticação ---
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        // Opções de configuração do Identity
    })
    .AddEntityFrameworkStores<ApexFoodDbContext>()
    .AddDefaultTokenProviders();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    });

    builder.Services.AddAuthorization();

    // --- Serviços da Camada de Aplicação ---
    builder.Services.AddMediatR(cfg =>
       cfg.RegisterServicesFromAssemblies(
           typeof(Program).Assembly, // Escaneia o projeto da API
           typeof(RegisterCommand).Assembly // Escaneia o projeto da Aplicação
       ));

    // --- Outros Serviços ---
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("A aplicação está funcional."));

    // 3. Configuração do Pipeline de Middlewares HTTP
    var app = builder.Build();

    // ... (O restante do arquivo continua o mesmo)

    // 5. Execução
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

// Expõe a classe Program para a WebApplicationFactory dos testes de integração
public partial class Program { }