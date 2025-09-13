// ApexFood.Api/Program.cs

using ApexFood.Application.Common.Interfaces;
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Application.Contracts.Authentication;
using ApexFood.Application.Features.Authentication;
using ApexFood.Domain.Entities;
using ApexFood.Infrastructure.Authentication;
using ApexFood.Infrastructure.Services;
using ApexFood.Persistence.Data;
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

// 1. CONFIGURAÇÃO INICIAL E LOGGING (BOOTSTRAP)
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

    // 2. REGISTRO DE SERVIÇOS NO CONTAINER DE INJEÇÃO DE DEPENDÊNCIA (DI)

    // --- Serviços de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Serviços de Persistência ---
    builder.Services.AddDbContext<ApexFoodDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString);
    });
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());

    // --- Serviços de Segurança e Autenticação ---
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApexFoodDbContext>()
    .AddDefaultTokenProviders();

    builder.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        });

    // --- Outros Serviços (Health Checks, etc.) ---
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("A aplicação está funcional."))
        // PASSO NOVO: Adiciona uma verificação de saúde para a conexão com o banco de dados.
        .AddDbContextCheck<ApexFoodDbContext>(name: "database");

    // --- Serviços da Camada de Aplicação ---
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));


    // 3. CONFIGURAÇÃO DO PIPELINE DE MIDDLEWARES HTTP
    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // 4. MAPEAMENTO DOS ENDPOINTS

    // --- Endpoints Públicos ---
    app.MapGet("/", () => "ApexFood API is running!");
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var options = new JsonSerializerOptions { WriteIndented = true };
            var payload = new { /* ... (código que já tínhamos) ... */ };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
        }
    });

    // --- Endpoints de Autenticação ---
    var authGroup = app.MapGroup("/auth").WithTags("Authentication");
    authGroup.MapPost("/register", async (RegisterRequest request, ISender mediator) => { /* ... */ });
    authGroup.MapPost("/login", async (LoginRequest request, ISender mediator) => { /* ... */ });

    // --- Endpoints Protegidos ---
    var meGroup = app.MapGroup("/me").WithTags("Me").RequireAuthorization();
    meGroup.MapGet("/context", (ITenantResolver tenantResolver) => { /* ... */ });

    // --- Endpoints de Negócio ---
    var insumosGroup = app.MapGroup("/insumos").WithTags("Insumos").RequireAuthorization();
    insumosGroup.MapGet("/", async (IApplicationDbContext context) =>
    {
        var insumos = await context.Insumos.ToListAsync();
        return Results.Ok(insumos);
    });

    // 5. EXECUÇÃO DA APLICAÇÃO
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