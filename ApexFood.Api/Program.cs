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
using ApexFood.Api.Services;
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

    // --- INÍCIO DA CONFIGURAÇÃO DO CORS ---
    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
                          policy =>
                          {
                              // Permite chamadas vindas do nosso servidor de frontend
                              policy.WithOrigins("http://localhost:5173")
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          });
    });

    // Força Kestrel a usar HTTP e HTTPS conforme o launchSettings.json
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5266); // HTTP
        options.ListenLocalhost(7086, listenOptions =>
        {
            listenOptions.UseHttps(); // HTTPS
        });
    });

    // --- FIM DA CONFIGURAÇÃO DO CORS ---

    // 1. Configuração de Logging (Serilog)
    // Só usar a configuração completa do Serilog (com appsettings, etc.)
    // se NÃO estivermos no ambiente de testes.
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
    // 2. Registro de Serviços no Container de DI
    
    // Add services to the container.
    builder.Services.AddControllers();

    // --- ADICIONA OS SERVIÇOS DO SWAGGER ---
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(); // <--- ESTA LINHA É ESSENCIAL
    builder.Services.AddSingleton<InsumoDataStore>();

    // --- Serviços de Infraestrutura ---
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ITenantResolver, TenantResolver>();

    // --- Serviços de Persistência ---
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddDbContext<ApexFoodDbContext>();
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApexFoodDbContext>());


    // Registra os Repositórios Genérico e Específicos
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();

    // --- Serviços de Segurança e Autenticação ---
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

    // --- Serviços da Camada de Aplicação ---
    builder.Services.AddMediatR(cfg =>
       cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

    // --- Outros Serviços ---
    builder.Services.AddHealthChecks();

    // 3. Configuração do Pipeline de Middlewares HTTP
    var app = builder.Build();
    
    // O middleware de request logging do Serilog também deve ser condicional.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseSerilogRequestLogging();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors(MyAllowSpecificOrigins);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // 4. Mapeamento dos Endpoints
    var authGroup = app.MapGroup("/auth").WithTags("Authentication");
    // (Endpoints de registro e login aqui)

    // ==================================================================
    // ENDPOINTS DE INSUMOS (Faltando no seu arquivo)
    // ==================================================================
    var insumosGroup = app.MapGroup("/insumos").WithTags("Insumos").RequireAuthorization();

    insumosGroup.MapPost("/", async (CriarInsumoRequest request, ISender mediator) =>
    {
        // Correção 1: Passa todos os parâmetros do request para o comando.
        var command = new CriarInsumoCommand(
            request.Nome,
            request.UnidadeMedidaBase,
            request.Gtin,
            request.Sku);

        var insumoId = await mediator.Send(command);

        // Correção 2: Usa o resultado (que é um Guid) diretamente.
        return Results.Created($"/insumos/{insumoId}", new { Id = insumoId });
    });

    insumosGroup.MapGet("/", async (IApplicationDbContext context) =>
    {
        var insumos = await context.Insumos.ToListAsync();
        return Results.Ok(insumos);
    });

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

public partial class Program { }