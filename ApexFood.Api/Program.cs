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

// Configura��o do Bootstrap Logger
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up ApexFood.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Configura��o de Logging (Serilog)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // 2. Registro de Servi�os no Container de DI

    // --- Servi�os de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Servi�os de Persist�ncia ---
    builder.Services.AddDbContext<ApexFoodDbContext>();
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());

    // --- Servi�os de Seguran�a e Autentica��o ---

    // 1. Configura os servi�os do Identity Core.
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

    // 2. Configura os esquemas de autentica��o.
    builder.Services.AddAuthentication(options =>
    {
        // Define o esquema de autentica��o padr�o como JwtBearer.
        // Isso impede que o Identity tente redirecionar para uma p�gina de login.
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
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("A aplica��o est� funcional."));

    // 3. Pipeline de Middlewares HTTP
    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // 4. Mapeamento de Endpoints

    // --- Endpoints P�blicos ---
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

    // --- Endpoints de Autentica��o ---
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

    var insumosGroup = app.MapGroup("/insumos").WithTags("Insumos").RequireAuthorization();
    insumosGroup.MapGet("/", async (IApplicationDbContext context) =>
    {
        var insumos = await context.Insumos.ToListAsync();
        return Results.Ok(insumos);
    });

    // 5. Execu��o da Aplica��o
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