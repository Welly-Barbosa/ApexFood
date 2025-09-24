// ApexFood.Api/Program.cs

using ApexFood.Api.Contracts.Insumos;
using ApexFood.Application.Common.Interfaces;
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Application.Common.Interfaces.Persistence;
using ApexFood.Application.Features.Authentication;
using ApexFood.Application.Features.Insumos;
using ApexFood.Domain.Entities;
using ApexFood.Infrastructure.Authentication;
using ApexFood.Infrastructure.Services;
using ApexFood.Persistence.Data;
using ApexFood.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up ApexFood.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Configura��o de Logging (Serilog)
    // S� usar a configura��o completa do Serilog (com appsettings, etc.)
    // se N�O estivermos no ambiente de testes.
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.Host.UseSerilog((context, services, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
                         .ReadFrom.Services(services)
                         .Enrich.FromLogContext()
        );
    }
    else
    {
        // No ambiente de teste, usamos um logger de console simples para evitar conflitos.
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
    }
    // 2. Registro de Servi�os no Container de DI

    // --- Servi�os de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Servi�os de Persist�ncia ---
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddDbContext<ApexFoodDbContext>();
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());


    // Registra os Reposit�rios Gen�rico e Espec�ficos
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();

    // --- Servi�os de Seguran�a e Autentica��o ---
    builder.Services.AddIdentity<User, IdentityRole<Guid>>()
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
       cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

    // --- Outros Servi�os ---
    builder.Services.AddHealthChecks();

    // 3. Configura��o do Pipeline de Middlewares HTTP
    var app = builder.Build();

    // O middleware de request logging do Serilog tamb�m deve ser condicional.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseSerilogRequestLogging();
    }
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // 4. Mapeamento dos Endpoints
    var authGroup = app.MapGroup("/auth").WithTags("Authentication");
    // (Endpoints de registro e login aqui)

    // ==================================================================
    // ENDPOINTS DE INSUMOS (Faltando no seu arquivo)
    // ==================================================================
    var insumosGroup = app.MapGroup("/insumos").WithTags("Insumos").RequireAuthorization();

    insumosGroup.MapPost("/", async (CriarInsumoRequest request, ISender mediator) =>
    {
        // Corre��o 1: Passa todos os par�metros do request para o comando.
        var command = new CriarInsumoCommand(
            request.Nome,
            request.UnidadeMedidaBase,
            request.Gtin,
            request.Sku);

        var insumoId = await mediator.Send(command);

        // Corre��o 2: Usa o resultado (que � um Guid) diretamente.
        return Results.Created($"/insumos/{insumoId}", new { Id = insumoId });
    });

    insumosGroup.MapGet("/", async (IApplicationDbContext context) =>
    {
        var insumos = await context.Insumos.ToListAsync();
        return Results.Ok(insumos);
    });

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

public partial class Program { }