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

    // 1. Configura��o de Logging (Serilog)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    // 2. Registro de Servi�os no Container de DI

    // --- Servi�os de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Servi�os de Persist�ncia (SE��O CORRIGIDA) ---
    builder.Services.AddDbContext<ApexFoodDbContext>(options =>
    {
        // L� a string de conex�o do appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Configura o DbContext para usar o SQL Server com a string de conex�o.
        options.UseSqlServer(connectionString);
    });
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());

    // ==================================================================
    // SE��O ADICIONADA: Registra os Reposit�rios.
    // ==================================================================
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();

    // --- Servi�os de Seguran�a e Autentica��o ---
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        // Op��es de configura��o do Identity
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

    // --- Servi�os da Camada de Aplica��o ---
    builder.Services.AddMediatR(cfg =>
       cfg.RegisterServicesFromAssemblies(
           typeof(Program).Assembly, // Escaneia o projeto da API
           typeof(RegisterCommand).Assembly // Escaneia o projeto da Aplica��o
       ));

    // --- Outros Servi�os ---
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("A aplica��o est� funcional."));

    // 3. Configura��o do Pipeline de Middlewares HTTP
    var app = builder.Build();

    // ... (O restante do arquivo continua o mesmo)

    // 5. Execu��o
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

// Exp�e a classe Program para a WebApplicationFactory dos testes de integra��o
public partial class Program { }