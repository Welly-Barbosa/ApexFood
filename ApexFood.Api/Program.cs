// ApexFood.Api/Program.cs

using ApexFood.Application.Common.Interfaces;
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Application.Common.Interfaces.Persistence;
using ApexFood.Application.Contracts.Authentication;
using ApexFood.Application.Features.Authentication;
using ApexFood.Application.Features.Insumos;
using ApexFood.Application.Common.Behaviors;
using ApexFood.Api.Contracts.Insumos;
using ApexFood.Domain.Entities;
using ApexFood.Infrastructure.Authentication;
using ApexFood.Infrastructure.Services;
using ApexFood.Persistence.Data;
using ApexFood.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json;


// Configuração do Bootstrap Logger
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up ApexFood.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Configuração de Logging (Serilog)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // 2. Registro de Serviços no Container de DI

    // --- Serviços de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Serviços de Persistência ---
    // builder.Services.AddDbContext<ApexFoodDbContext>(); // Linha comentada no PASSO antigo
    builder.Services.AddDbContext<ApexFoodDbContext>(options =>
    {
        // Lê a string de conexão do appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Configura o DbContext para usar o SQL Server com a string de conexão.
        options.UseSqlServer(connectionString);
    });
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());

    // PASSO NOVO: Registra os repositórios
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();

    // --- Serviços de Segurança e Autenticação ---

    // 1. Configura os serviços do Identity Core.
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApexFoodDbContext>()
    .AddDefaultTokenProviders();

    // 2. Configura os esquemas de autenticação.
    builder.Services.AddAuthentication(options =>
    {
        // Define o esquema de autenticação padrão como JwtBearer.
        // Isso impede que o Identity tente redirecionar para uma página de login.
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
        cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

    // --- Serviços da Camada de Aplicação ---
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(CriarInsumoCommand).Assembly));

    // Adiciona o ValidationBehavior ao pipeline do MediatR
    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Registra os validadores do FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(CriarInsumoCommandValidator).Assembly);

    // --- Registra os validadores do FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(CriarInsumoCommand).Assembly);

    // --- Outros Serviços ---
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("A aplicação está funcional."));

    // 3. Pipeline de Middlewares HTTP
    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // 4. Mapeamento de Endpoints

    // --- Endpoints Públicos ---
    var publicGroup = app.MapGroup("").WithTags("Public");
    publicGroup.MapGet("/", () => "ApexFood API is running!");
    publicGroup.MapHealthChecks("/health", new HealthCheckOptions
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

    // ==================================================================
    // PASSO NOVO: Cria o endpoint de Insumos
    // ==================================================================
    var insumosGroup = app.MapGroup("/insumos").WithTags("Insumos").RequireAuthorization();

    insumosGroup.MapPost("/", async (CriarInsumoRequest request, ISender mediator) =>
    {
        var command = new CriarInsumoCommand(
            request.Nome,
            request.UnidadeMedidaBase,
            request.Gtin,
            request.Sku);

        var insumoId = await mediator.Send(command);

        return Results.Created($"/insumos/{insumoId}", new { Id = insumoId });
    })
    .Produces<object>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

    var insumosGroup1 = app.MapGroup("/insumos").WithTags("Insumos").RequireAuthorization();
    insumosGroup1.MapGet("/", async (IApplicationDbContext context) =>
    {
        var insumos = await context.Insumos.ToListAsync();
        return Results.Ok(insumos);
    });

    // --- Endpoints de Autenticação ---
    var authGroup = app.MapGroup("/auth").WithTags("Authentication");

    authGroup.MapPost("/register", (RegisterRequest request, ISender mediator) =>
    {
        var command = new RegisterCommand(request.Email, request.Password);
        var result = mediator.Send(command).Result;
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

    authGroup.MapPost("/login", (LoginRequest request, ISender mediator) =>
    {
        try
        {
            var query = new LoginQuery(request.Email, request.Password);
            var authResult = mediator.Send(query).Result;
            return Results.Ok(authResult);
        }
        catch (Exception)
        {
            return Results.Unauthorized();
        }
    });

    // --- Endpoints Protegidos ---
    var meGroup = app.MapGroup("/me").WithTags("Me").RequireAuthorization();
    meGroup.MapGet("/context", (ITenantResolver tenantResolver) =>
    {
        var tenantId = tenantResolver.GetTenantId();
        return Results.Ok(new { TenantId = tenantId });
    });



    // 5. Execução da Aplicação
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